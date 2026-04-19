문서명: Week5_CalculationValidation_v1

목적
현재 구현된 CircuitCalculationHelper가
의도한 교육용 계산 규칙대로 동작하는지 검증한다.

--------------------------------

1. 검증 범위

이번 단계에서 검증할 것은 아래다.

- 저항 등록이 정상인지
- active resistor 판정이 정상인지
- 직렬 회로 계산이 정상인지
- 병렬 회로 계산이 정상인지
- LedOn / Open 등 상태별 계산 결과가 의도대로 나오는지

이번 단계에서는 UI / 이펙트 연동은 다루지 않는다.

--------------------------------

2. 검증 대상

핵심 대상:
- CircuitResistorRegistrar
- CircuitCalculationHelper
- CircuitCalculationResult

보조 확인:
- CircuitEvaluator
- CircuitContext.Resistors

--------------------------------

3. 기본 검증 포인트

1. 저항 등록
- 저항 오브젝트가 시작 시 CircuitResistor로 등록되는가
- 중복 등록이 없는가

2. active resistor 판정
- 실제 연결된 저항만 계산 대상에 포함되는가
- 연결되지 않은 저항은 제외되는가

3. 계산 결과
- totalVoltage
- totalCurrent
- totalResistance
- activeLoadCount
- perLoadVoltage
- perBranchCurrent

--------------------------------

4. 직렬 회로 검증 규칙

예시:
- 공급 전압 = 3.0V
- 저항 2개
  - R1 = 220Ω
  - R2 = 220Ω

기대값:
- totalResistance = 440Ω
- totalCurrent = 3 / 440
- 각 저항 전압 = totalCurrent × 220
- perLoadVoltage 두 값이 동일

--------------------------------

5. 병렬 회로 검증 규칙

예시:
- 공급 전압 = 3.0V
- 저항 2개
  - R1 = 220Ω
  - R2 = 220Ω

기대값:
- totalResistance = 110Ω
- totalCurrent = 3 / 110
- 각 branch 전압 = 3.0V
- 각 branch current = 3 / 220

--------------------------------

6. 예외 검증 규칙

1. Open 상태
- 계산 결과 invalid

2. active resistor 0개
- 계산 결과 invalid

3. 저항값 0
- 계산 결과 invalid 또는 해당 저항 제외

4. LedOn 상태
- 현재 단계에서는 active resistor가 있으면 계산 허용
- 단순 series-style aggregation 적용

--------------------------------

7. 검증 방식

권장 방식:
- 디버그 로그 기반 임시 검증
- 또는 별도 테스트 러너로 결과 출력

출력 예시:
- state
- resistor count
- totalVoltage
- totalResistance
- totalCurrent
- perLoadVoltage
- perBranchCurrent

--------------------------------

8. 완료 기준

1. 직렬 회로에서 기대값과 계산값이 일치한다
2. 병렬 회로에서 기대값과 계산값이 일치한다
3. invalid 상황에서 invalid result가 나온다
4. 저항 등록 및 active resistor 판정이 정상이다

--------------------------------

9. 핵심 한 줄 요약

이번 단계의 핵심은
"현재 계산 helper가 직렬/병렬/예외 상황에서 올바른 값을 내는지 검증하는 것"이다.