# CircuitSystem_Week6_Extra_1.BreadboardCurrentVisualIntegration_v1

## 목적

점퍼와이어 전류 이펙트 외에도, 브레드보드 중앙 hole 또는 pin 그룹에 전류 흐름 시각화를 적용한다.

이미 전류 흐름 시각화용 스크립트는 별도로 준비되어 있으므로,
이번 작업은 회로 엔진의 전류 흐름 결과와 해당 스크립트를 연결하는 것이다.

---

## 현재 완료 상태

- CircuitCurrentFlowAnalyzer가 전류 흐름 여부를 판정함
- WireDirections를 통해 + rail → - rail 방향의 node path를 계산함
- JumperWireLight 이펙트 연동 완료
- active wire만 LightOn / LightOff 가능

---

## 이번 작업 목표

브레드보드의 중앙 row node에 전류가 흐를 때,
해당 node와 연결된 시각화 스크립트를 켠다.

예:

BottomPlus_2
→ Green wire
→ LowerRow_17
→ Resistor
→ LowerRow_19
→ Red wire
→ BottomMinus_2

이 경우 전류가 흐르는 중앙 node:
- LowerRow_17
- LowerRow_19

따라서 해당 row 또는 pin group의 전류 시각화를 ON 한다.

---

## 설계 방향

새 연동 스크립트:

BreadboardCurrentVisualController

역할:
1. CircuitCurrentFlowAnalyzer.Analyze() 실행
2. 전류가 흐르는 node 목록 추출
3. 브레드보드 nodeId와 시각화 스크립트를 매핑
4. active node는 ON
5. inactive node는 OFF

---

## 주의사항

- 기존 전류 계산 로직 수정 금지
- CircuitEvaluator 수정 금지
- JumperWireLight 수정 금지
- 브레드보드 시각화 스크립트 자체 수정 최소화
- 연결 컨트롤러만 새로 작성

---

## 연동 기준

현재 회로 엔진은 nodeId 기준으로 회로를 판정한다.

따라서 브레드보드 시각화도 nodeId 기준으로 연결한다.

예:
- LowerRow_17
- LowerRow_19
- UpperRow_3
- BottomPlus_2
- BottomMinus_2

---

## 완료 기준

1. 회로가 닫히면 active node의 브레드보드 시각화가 켜진다.
2. 회로가 열리면 모든 브레드보드 시각화가 꺼진다.
3. 점퍼와이어 이펙트와 동시에 동작한다.
4. 기존 계산 결과에 영향이 없다.
5. UI 연동 구조와 충돌하지 않는다.