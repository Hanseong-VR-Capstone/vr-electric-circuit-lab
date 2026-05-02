# CircuitSystem_Week6_Extra_2.LedSwitchRegistration_v1

## 목적

현재 회로 시스템에는 Wire와 Resistor 등록 구조는 존재하지만,
LED와 Switch는 CircuitContext에 회로 부품 데이터로 등록되지 않는다.

따라서 LED/Switch도 회로 판정 및 전류 흐름 분석에 참여할 수 있도록 등록용 스크립트를 추가한다.

---

## 현재 구조

이미 존재:
- CircuitContext.Leds
- CircuitContext.Switches
- AddLed()
- AddSwitch()

이미 사용 중:
- CircuitEvaluator는 context.Leds로 LedOn 판정
- CircuitEvaluator는 context.Switches로 switch edge 추가
- CircuitCurrentFlowAnalyzer도 context.Switches를 edge로 사용
- CircuitConnectionService.SwitchStateChanged(switchId, isOn) 존재

현재 없는 것:
- CircuitLedRegistrar
- CircuitSwitchRegistrar
- SwitchPart 상태와 CircuitSwitch.IsOn 동기화

---

## 필요한 작업

1. CircuitLedRegistrar 추가
2. CircuitSwitchRegistrar 추가
3. Switch 상태 동기화 구조 추가

---

## LED 등록 방향

LedPart의 PartPin 2개를 읽어서 CircuitLed 생성.

예상 구조:
- ledId
- anodePinId
- cathodePinId

CircuitContext.AddLed() 호출.

주의:
- LED 시각효과 연동은 이번 단계에서 제외
- 이번 단계는 회로 데이터 등록만 담당

---

## Switch 등록 방향

SwitchPart의 PartPin 2개를 읽어서 CircuitSwitch 생성.

예상 구조:
- switchId
- pinAId
- pinBId
- isOn

CircuitContext.AddSwitch() 호출.

---

## Switch 상태 동기화

SwitchPart.isOn이 변경될 때 CircuitSwitch.IsOn에도 반영되어야 한다.

가능한 방식:
1. CircuitSwitchStateBridge 추가
2. 또는 CircuitSwitchRegistrar가 Update에서 SwitchPart.isOn 변화를 감지
3. 변화 발생 시 CircuitConnectionService.SwitchStateChanged(switchId, isOn) 호출

초기 구현은 단순하게 Update 감지 방식으로 진행한다.

---

## 완료 기준

1. context.Leds.Count 증가
2. context.Switches.Count 증가
3. LED pin이 연결되면 Evaluator에서 LedOn 판정 가능
4. Switch가 OFF이면 edge 없음
5. Switch가 ON이면 edge 추가
6. Switch 상태 변경 시 회로 상태가 다시 평가됨

---

## 다음 단계

1. LED/Switch 등록 코드 작성
2. 등록 로그 확인
3. Switch ON/OFF 회로 판정 테스트
4. LED visual 연동은 이후 별도 진행