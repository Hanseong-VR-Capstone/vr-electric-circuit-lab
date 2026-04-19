문서명: Week5_ResistorRegistrationBridge_v1

목적
VR 저항 파트를 회로 엔진의 CircuitResistor 데이터와 연결하는 등록 구조를 설계한다.

--------------------------------

1. 현재 상태

현재 시스템은 다음을 이미 가진다.

- PartPin -> CircuitPin 자동 생성
- hole -> socket 연결
- rail 기반 evaluator
- CircuitResistor 데이터 구조
- CircuitContext.AddResistor(...)
- CircuitCalculationHelper

하지만 아직 VR 저항 파트가
자동으로 CircuitResistor로 등록되지는 않는다.

즉 현재는:
- 핀 연결은 됨
- 저항 계산 데이터 등록은 아직 안 됨

--------------------------------

2. 이번 단계 핵심 목표

저항 파트가 씬에 존재할 때,
해당 파트의 두 핀과 저항값을 이용해
CircuitResistor를 CircuitContext에 등록할 수 있게 한다.

즉 다음 연결을 만든다.

VR Resistor Part
-> PartPin 2개
-> resistorId / pinAId / pinBId / resistanceOhms 해석
-> CircuitResistor 생성
-> CircuitContext.AddResistor(...)

--------------------------------

3. 설계 원칙

1. BreadboardHoleBridge는 그대로 유지한다
2. CircuitEvaluator는 수정하지 않는다
3. CircuitCalculationHelper는 수정하지 않는다
4. 저항 등록은 별도 등록기(registrar)로 분리한다
5. 중복 등록 방지 규칙을 둔다

--------------------------------

4. 필요한 신규 계층

권장 클래스:
- CircuitResistorRegistrar

역할:
- 저항 파트 정보를 읽는다
- resistorId 해석
- 양 끝 pinId 해석
- resistanceOhms 해석
- CircuitContext에 등록

--------------------------------

5. 입력 정보

저항 등록에 필요한 최소 정보:

- resistorId
- pinAId
- pinBId
- resistanceOhms

해석 기준:

resistorId
- 저항 파트 고유 이름 또는 별도 adapter 값

pinAId / pinBId
- 각 저항 PartPin의 CircuitPinIdAdapter 우선 사용
- 없으면 기존 fallback 규칙 사용 가능

resistanceOhms
- ResistorPart가 가진 resistance 값 사용

--------------------------------

6. 등록 시점

권장 방식은 두 가지다.

A안
- 저항 파트 초기화 시 등록
- Start / Awake 이후 한 번 등록

B안
- 저항 파트가 처음 hole과 상호작용할 때 등록 보장

현재 단계 권장:
- 별도 registrar를 저항 파트 쪽에 붙이고
- Start 또는 명시적 Initialize 시점에 등록

이유:
- BreadboardHoleBridge 책임을 늘리지 않기 위함

--------------------------------

7. 중복 등록 방지

중요:
같은 저항이 여러 번 AddResistor 되면 안 된다.

따라서 registrar는 등록 전에 아래를 확인한다.

- context.GetResistorById(resistorId) != null 이면 등록하지 않음

--------------------------------

8. CircuitRuntimeRoot와의 연결

registrar는 shared CircuitContext에 접근해야 한다.

권장 방식:
- CircuitRuntimeRoot 참조
- runtimeRoot.Context 사용

즉 registrar는:
- runtimeRoot.Context
- resistor part 정보
를 이용해서 등록한다.

--------------------------------

9. 저항 파트 전제

현재 저항 파트에는 최소한 아래가 있다고 가정한다.

- ResistorPart
- resistance 값
- 두 개의 PartPin

필요 시 별도 adapter를 추가할 수 있다.

예:
- CircuitResistorIdAdapter
- 또는 직접 resistorId 문자열 필드

--------------------------------

10. 완료 기준

1. 저항 파트 1개가 CircuitResistor로 등록된다
2. resistorId 중복 등록이 발생하지 않는다
3. pinAId / pinBId가 기존 pinId 규칙과 일치한다
4. CircuitCalculationHelper가 context.Resistors를 읽을 수 있다

--------------------------------

11. 핵심 한 줄 요약

이번 단계의 핵심은
"VR 저항 파트를 회로 엔진의 CircuitResistor 데이터로 등록하는 것"이다.