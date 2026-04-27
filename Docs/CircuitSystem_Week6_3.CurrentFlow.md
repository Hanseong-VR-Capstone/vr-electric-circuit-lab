# CircuitSystem_Week6_2.CurrentFlowDirection_v1

## 목적

현재 회로 엔진은 회로가 연결되었는지, Series/Parallel인지, 계산값이 무엇인지는 정상 판정한다.

지금까지 완료된 것:
- RuntimeRoot 초기화 순서 문제 해결
- Breadboard socket 460개 등록 정상화
- PartPin → CircuitPin 생성 정상화
- JumperWire → CircuitWire 등록 추가
- Wire pinId 중복 문제 해결
- Resistor edge를 graph에 포함
- 단일 저항 회로 Series 판정 성공
- 10KΩ 저항 2개 병렬 회로 Parallel 판정 및 계산 성공

다음 단계는 전류 흐름 이펙트 연동을 위해 “어느 wire가 어느 방향으로 흐르는지”를 계산하는 것이다.

---

## 현재 상태

현재 CircuitCurrentFlowAnalyzer는 다음만 판단한다.

- IsFlowing
- ActiveWireIds

하지만 이펙트 방향을 맞추려면 다음 정보가 필요하다.

- wireId
- fromNodeId
- toNodeId
- fromSocketId
- toSocketId
- fromPinId
- toPinId

---

## 설계 원칙

전류 방향은 교육용 단순 기준으로 설정한다.

기준:
+ rail → - rail

즉 실제 전자 흐름이 아니라, 일반적인 회로 교육에서 쓰는 관습적 전류 방향을 따른다.

---

## 목표

CircuitCurrentFlowAnalyzer가 rail-to-rail path를 찾은 뒤,
그 path에 포함된 wire마다 방향 정보를 만든다.

예시:

BottomPlus_2
→ LowerRow_17
→ LowerRow_19
→ BottomMinus_2

이 path에서 wire가 다음과 같다면:

Green wire:
BottomPlus_2 → LowerRow_17

Red wire:
LowerRow_19 → BottomMinus_2

이런 식으로 wire별 방향을 반환한다.

---

## 결과 구조 제안

새 클래스:

CurrentFlowWireDirection

필드:
- wireId
- fromNodeId
- toNodeId
- fromPinId
- toPinId
- fromSocketId
- toSocketId

CircuitCurrentFlowResult에 추가:
- IReadOnlyList<CurrentFlowWireDirection> WireDirections

기존 유지:
- IsFlowing
- ActiveWireIds

---

## 주의사항

1. 기존 ActiveWireIds는 유지한다.
2. Resistor는 path에는 포함하지만 WireDirections에는 넣지 않는다.
3. 이펙트는 wire에만 적용한다.
4. 계산 로직은 건드리지 않는다.
5. Evaluator 로직은 건드리지 않는다.
6. UI 로직은 아직 추가하지 않는다.

---

## 완료 기준

1. 회로가 연결되면 IsFlowing = true
2. ActiveWireIds에 흐르는 wire id가 들어간다.
3. WireDirections에 wire별 방향 정보가 들어간다.
4. 단일 저항 회로에서 +rail 쪽 wire와 -rail 쪽 wire 방향이 정상 출력된다.
5. 병렬 회로는 우선 발견된 rail-to-rail path 기준으로 방향을 출력한다.
6. 이후 필요하면 병렬 전체 branch 방향 계산으로 확장한다.

---

## 다음 단계

1. CurrentFlowAnalyzer 방향 정보 추가
2. 디버그 출력으로 방향 확인
3. Wire 이펙트 스크립트와 연결
4. 계산 결과 UI 연동