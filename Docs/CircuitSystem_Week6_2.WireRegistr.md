# CircuitSystem_Week6_1.WireRegistrationSystem_v1

## 목적

현재 VR 회로 시스템에서 점퍼와이어(JumperWire)가 CircuitContext에 CircuitWire로 등록되지 않아
회로 graph에 edge가 생성되지 않는 문제를 해결한다.

이 문서는 Wire 등록 시스템을 추가하여,
실제 회로 연결이 graph 기반 evaluator에 반영되도록 한다.

---

## 지금까지 진행된 작업 요약

### 1. 초기 문제
- 저항 포함 회로가 항상 Open으로 판정됨
- 원인: evaluator graph에 resistor edge가 없음

### 2. 수정 1 (Evaluator)
- CircuitEvaluator에 AddResistorEdges 추가
- CircuitCurrentFlowAnalyzer에도 동일 반영

### 3. 문제 재발견
- 여전히 Open 발생
- 디버그 결과:
  - Sockets.Count = 0 → Bootstrap 초기화 실패

### 4. 수정 2 (초기화 문제 해결)
- CircuitRuntimeRoot.EnsureInitialized() 추가
- BreadboardSocketBootstrap을 Start()에서 실행하도록 변경
- 결과:
  - Sockets.Count = 460 정상화

### 5. 현재 상태
- PartPin → CircuitPin 생성 정상
- Pin → Socket 연결 정상
- Resistor → CircuitResistor 등록 정상

하지만:

```text
Wires.Count = 0