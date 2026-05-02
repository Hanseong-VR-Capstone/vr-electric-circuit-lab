# CircuitSystem_Week6_Extra_3.LedEffectIntegration_v1

## 목적
회로 엔진의 LedOn 판정 결과와 LedPart.manageLight(bool)을 연결한다.

## 현재 상태
- CircuitLedRegistrar로 LED 회로 데이터 등록 가능
- CircuitEvaluator가 조건 충족 시 CircuitState.LedOn 반환 가능
- LedPart는 manageLight(bool isOn)으로 시각 효과 제어 가능
- 하지만 LedOn 판정과 LedPart.manageLight(...)를 연결하는 컨트롤러는 아직 없음

## 추가할 구조
CircuitLedEffectController

## 역할
1. CircuitEvaluator.Evaluate() 실행
2. 결과가 CircuitState.LedOn이면 LED 이펙트 ON
3. 아니면 LED 이펙트 OFF
4. 중복 호출 방지

## 완료 기준
- LED 방향이 맞고 회로가 닫히면 LED 이펙트 ON
- 회로가 열리거나 방향이 틀리면 LED 이펙트 OFF