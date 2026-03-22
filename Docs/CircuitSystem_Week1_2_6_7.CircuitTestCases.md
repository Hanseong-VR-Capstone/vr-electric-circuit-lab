VR Electric Circuit System
Day 6-7 Design – Test Cases and Validation Rules

목적
현재 구현된 데이터 계층, 서비스 계층, 평가 계층이
교육용 회로 규칙에 맞게 동작하는지 검증하기 위한 테스트 케이스를 정의한다.

현재 평가기는 다음 상태를 반환한다.

- Open
- LedOn
- Short

Closed 상태는 현재 버전에서 예약 상태이며,
실제 반환에는 아직 사용되지 않는다.

--------------------------------

1. 테스트 목적

테스트의 목적은 다음과 같다.

1. 연결 이벤트가 정상적으로 반영되는지 확인
2. Evaluate() 호출 후 상태가 기대값과 일치하는지 확인
3. LED 극성 판정이 정상인지 확인
4. 스위치 on/off가 상태 변화에 반영되는지 확인
5. 잘못된 연결이나 예외 상황에서 시스템이 무너지지 않는지 확인

--------------------------------

2. 공통 전제

테스트에서는 아래 구성 요소가 있다고 가정한다.

- Battery 1개
- LED 1개
- Wire 여러 개
- 필요 시 Switch 1개
- 브레드보드 row / rail socket

기본 식별자 예시

Battery
- Battery1
- Battery_Pos
- Battery_Neg

LED
- LED1
- LED1_Anode
- LED1_Cathode

Switch
- Switch1
- Switch1_A
- Switch1_B

Node 예시
- RailPlus
- RailMinus
- Row1_L
- Row1_R
- Row2_L
- Row2_R

--------------------------------

3. 핵심 상태 테스트

Test 1 – 아무것도 연결되지 않은 상태

설명
- 배터리 핀도 연결 안 됨
- LED도 연결 안 됨
- 와이어도 연결 안 됨

기대 결과
- CircuitState.Open

--------------------------------

Test 2 – 배터리 + / - 만 각각 소켓에 꽂혀 있으나 경로 없음

설명
- Battery_Pos -> RailPlus
- Battery_Neg -> RailMinus
- 그 외 연결 없음

기대 결과
- CircuitState.Open

--------------------------------

Test 3 – 정상 LED 회로

설명 예시
- Battery_Pos -> RailPlus
- Wire: RailPlus -> LED1_Anode 쪽 node
- LED1_Cathode 쪽 node -> Wire -> RailMinus
- Battery_Neg -> RailMinus

즉
battery+ → anode → cathode → battery-

기대 결과
- CircuitState.LedOn

--------------------------------

Test 4 – LED 반대로 연결한 경우

설명 예시
- Battery_Pos가 LED cathode 방향으로 연결됨
- LED anode 방향이 Battery_Neg 쪽으로 감

즉
battery+ → cathode
anode → battery-

기대 결과
- CircuitState.Short

이유
- 폐회로는 존재할 수 있으나
- 현재 규칙상 정상 LED 점등 조건이 아니므로 Short 처리

--------------------------------

Test 5 – 배터리 + 와 -를 와이어로 직접 연결

설명
- Battery_Pos -> Wire -> Battery_Neg 방향의 직접 연결
- LED 없음

기대 결과
- CircuitState.Short

--------------------------------

Test 6 – LED 한쪽 다리만 연결된 경우

설명
- anode만 연결됨
- cathode는 떠 있음

또는 반대

기대 결과
- CircuitState.Open

--------------------------------

Test 7 – 스위치가 꺼져 있어서 회로가 끊긴 경우

설명
- 정상 LED 회로와 거의 같지만
- 중간 스위치가 off

기대 결과
- CircuitState.Open

--------------------------------

Test 8 – 스위치를 켜면 정상 회로가 되는 경우

설명
- Test 7과 동일 구조
- SwitchStateChanged(..., true)

기대 결과
- CircuitState.LedOn

--------------------------------

Test 9 – 이미 점유된 socket에 다른 pin 연결 시도

설명
- 한 socket에 이미 pin A가 연결됨
- 다른 pin B를 같은 socket에 연결 시도

기대 결과
- ConnectPinToSocket 반환 false
- 기존 연결 유지
- 상태 무결성 유지

--------------------------------

Test 10 – 존재하지 않는 pinId 연결 시도

설명
- 없는 pinId로 ConnectPinToSocket 호출

기대 결과
- false 반환
- Debug warning 가능
- 상태 변화 없음

--------------------------------

Test 11 – 존재하지 않는 socketId 연결 시도

설명
- 없는 socketId로 ConnectPinToSocket 호출

기대 결과
- false 반환
- 상태 변화 없음

--------------------------------

Test 12 – 연결되지 않은 pin 해제 시도

설명
- currentSocketId가 null인 pin에 대해 DisconnectPin 호출

기대 결과
- false 반환
- 상태 변화 없음

--------------------------------

4. 검증 포인트

각 테스트에서 반드시 확인할 것

1. 메서드 반환값
- true / false

2. Pin 상태
- currentSocketId 값

3. Socket 상태
- connectedPinId 값

4. 서비스 계층 상태
- CurrentState 값

5. 예상치 못한 예외 발생 여부
- NullReferenceException 등 없어야 함

--------------------------------

5. 우선 테스트 순서

권장 순서

1. Open 계열 먼저
- Test 1
- Test 2
- Test 6
- Test 7

2. 정상 점등
- Test 3
- Test 8

3. Short 계열
- Test 4
- Test 5

4. 예외 입력
- Test 9
- Test 10
- Test 11
- Test 12

--------------------------------

6. 현재 버전의 한계

현재 버전은 교육용 단순 판정 구조다.

따라서
- 저항 계산 없음
- 전류량 계산 없음
- 다중 배터리 정밀 계산 없음
- 복잡한 병렬 회로 정밀 해석 없음

현재 테스트는
“연결 구조 기반 상태 판정”
검증에 집중한다.

--------------------------------

7. 이번 단계 핵심

이 단계의 핵심은
코드를 더 복잡하게 만드는 것이 아니라
현재 규칙이 실제로 유지되는지 확인하는 것이다.

즉
입력 시나리오
→ 연결 상태 변화
→ CurrentState 확인

이 흐름을 테스트 케이스로 고정하는 것이 목적이다.