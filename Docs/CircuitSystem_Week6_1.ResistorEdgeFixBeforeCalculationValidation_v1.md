# CircuitSystem_Week5_6.ResistorEdgeFixBeforeCalculationValidation_v1

## 목적

현재 계산 검증 단계에서 저항이 포함된 회로가 계속 Open으로 판정되는 문제를 해결한다.

문제의 핵심은 저항이 CircuitResistor 데이터로 등록되어 계산 helper에서는 사용할 수 있지만,
CircuitEvaluator의 node graph에서는 edge로 포함되지 않는다는 점이다.

즉 실제 회로는 아래처럼 연결되어 있어도,

plus rail
-> wire
-> node A
-> resistor
-> node B
-> wire
-> minus rail

Evaluator graph가 wire / switch만 edge로 보면 resistor 구간이 끊긴 것으로 판단되어 Open이 된다.

따라서 저항을 계산 데이터로 유지하면서도,
회로 연결 판정에서는 node와 node를 잇는 edge로 참여시켜야 한다.

---

## 현재 문제

현재 graph edge 대상:
- wire
- active switch

현재 누락된 edge 대상:
- resistor

결과:
- 저항 등록은 됨
- 저항 pinA / pinB도 socket에 연결됨
- 하지만 evaluator는 resistor 양 끝 node를 연결하지 않음
- 따라서 rail plus -> rail minus 경로가 끊긴 것으로 판단됨
- 계산 검증 전에 상태가 Open이 되어 계산이 invalid 처리됨

---

## 수정 목표

1. CircuitEvaluator.BuildNodeGraph()에 resistor edge를 추가한다.
2. CircuitCurrentFlowAnalyzer도 경로 탐색 시 resistor edge를 고려한다.
3. 단, CurrentFlowAnalyzer의 출력은 기존처럼 active wire id만 반환한다.
4. CircuitCalculationHelper의 계산식은 건드리지 않는다.
5. BreadboardHoleBridge, CircuitConnectionService, CircuitResistorRegistrar는 수정하지 않는다.

---

## 핵심 설계

저항은 두 가지 의미를 가진다.

1. 계산 관점
- resistanceOhms 값을 가진 계산 대상 부품

2. 그래프 판정 관점
- pinA가 연결된 node와 pinB가 연결된 node를 이어주는 edge

따라서 resistor는 wire로 바꾸면 안 된다.
resistor는 CircuitResistor로 유지하되,
BuildNodeGraph()에서만 node edge로 추가한다.

---

## 수정 대상

수정 가능 파일:
- CircuitEvaluator.cs
- CircuitCurrentFlowAnalyzer.cs

필요 시 최소 수정 가능:
- 관련 private helper 함수

수정 금지 파일:
- CircuitContext.cs
- CircuitConnectionService.cs
- BreadboardHoleBridge.cs
- BreadboardNodeMapper.cs
- BreadboardSocketBootstrap.cs
- CircuitResistorRegistrar.cs
- CircuitCalculationHelper.cs

---

## CircuitEvaluator 수정 방향

BuildNodeGraph() 내부 또는 근처에 AddResistorEdges() helper를 추가한다.

동작 규칙:

1. context.Resistors를 순회한다.
2. resistor.pinAId로 nodeA를 찾는다.
3. resistor.pinBId로 nodeB를 찾는다.
4. 둘 다 유효하면 graph에 양방향 edge를 추가한다.
5. nodeA == nodeB인 경우는 무시해도 된다.
6. pin 또는 socket이 연결되지 않은 resistor는 edge로 추가하지 않는다.

예상 구조:

- AddWireEdges(graph)
- AddSwitchEdges(graph)
- AddResistorEdges(graph)

BuildNodeGraph()는 위 세 edge를 모두 포함해야 한다.

---

## CircuitCurrentFlowAnalyzer 수정 방향

CurrentFlowAnalyzer가 plus rail -> minus rail 경로를 찾을 때도 resistor edge를 포함해야 한다.

단, 출력은 기존과 동일하게 wire id만 반환한다.

즉 path 탐색에는 resistor edge가 들어가야 하지만,
activeWireIds에는 resistor id를 넣지 않는다.

이유:
- 이펙트는 점퍼와이어에만 적용할 예정
- resistor는 경로 연결에는 필요하지만 wire 이펙트 대상은 아님

---

## 수정 후 기대 결과

저항 1개 회로:
plus rail
-> wire
-> resistor
-> wire
-> minus rail

기대:
- Open이 아님
- Series 또는 계산 가능한 상태
- active resistor 1개
- totalResistance = 해당 저항값
- totalCurrent = railVoltage / totalResistance

저항 2개 직렬 회로:
- totalResistance = R1 + R2
- totalCurrent = V / totalResistance
- 각 저항 전압 = I * R

저항 2개 병렬 회로:
- totalResistance = 1 / (1/R1 + 1/R2)
- 각 저항 전압 = railVoltage
- 각 branch current = V / R

---

## 주의사항

1. 저항을 wire 목록에 넣으면 안 된다.
2. CircuitResistor 구조를 변경하지 않는다.
3. 계산 helper를 먼저 고치지 않는다.
4. 현재 문제는 계산식 문제가 아니라 graph 연결 판정 문제다.
5. Evaluate와 CalculationHelper의 책임 분리를 유지한다.
6. resistor edge 추가 후 계산 검증을 다시 진행한다.

---

## 완료 기준

1. 저항 포함 회로가 더 이상 무조건 Open으로 나오지 않는다.
2. CircuitEvaluator graph에 resistor edge가 포함된다.
3. CurrentFlowAnalyzer 경로 탐색도 resistor를 통과할 수 있다.
4. active wire 출력은 기존처럼 wire id만 유지된다.
5. 직렬 계산 검증을 다시 진행할 수 있다.
6. 병렬 계산 검증을 다시 진행할 수 있다.

---

## 다음 단계

1. Codex로 resistor edge 추가 수정
2. 수정 코드 검토
3. Unity에서 저항 1개 회로 테스트
4. 직렬 저항 2개 계산 검증
5. 병렬 저항 2개 계산 검증
6. 전류 흐름 이펙트 연동
7. 계산 결과 UI 연동