# CircuitSystem_Week6_4.UIQueryService_v1

## 목적

UI가 아직 완성되지 않았기 때문에, 지금 단계에서는 UI를 직접 연결하지 않는다.

대신 A 역할은 회로 로직 결과를 UI가 쉽게 가져갈 수 있도록 조회용 API를 제공한다.

---

## 현재 완료 상태

- 회로 그래프 판정 정상
- 단일 저항 Series 계산 정상
- 2개 저항 Parallel 계산 정상
- 전류 흐름 방향 계산 정상
- JumperWireLight 이펙트 연동 정상

---

## 남은 작업

UI 연동

단, UI가 아직 완성되지 않았으므로 직접 Text/TMP에 연결하지 않고,
UI가 나중에 호출할 수 있는 함수만 제공한다.

---

## 새 구조

CircuitQueryService 추가

역할:
- 현재 회로 상태 반환
- 계산 결과 반환
- 전류 흐름 결과 반환
- UI용 요약 데이터 반환

---

## 제공 함수

- GetCircuitState()
- GetCalculationResult()
- GetCurrentFlowResult()
- GetUIData()

---

## UI용 데이터

CircuitUIData

포함 정보:
- State
- HasValidCircuit
- ActiveLoadCount
- TotalVoltage
- TotalResistance
- TotalCurrent
- PerLoadVoltage
- PerBranchCurrent
- IsFlowing
- ActiveWireIds

---

## 설계 원칙

- UI 오브젝트 직접 참조 금지
- TextMeshPro 직접 참조 금지
- Button 직접 참조 금지
- 계산 로직 수정 금지
- Evaluator 수정 금지
- 이펙트 로직 수정 금지
- UI가 나중에 가져다 쓰기 쉬운 형태로만 제공

---

## 완료 기준

1. UI에서 CircuitQueryService를 참조할 수 있다.
2. UI는 GetUIData() 하나로 주요 값을 받을 수 있다.
3. 기존 계산 결과와 동일한 값이 반환된다.
4. UI가 없어도 테스트 가능하다.