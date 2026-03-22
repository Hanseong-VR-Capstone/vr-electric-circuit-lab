VR Electric Circuit System
Day 1 Design – Event System

목적
회로 시스템에서 발생하는 모든 상태 변경은 "이벤트 기반"으로 처리한다.

회로 계산(Evaluate)은 이벤트 발생 후 실행된다.

--------------------------------

1. 이벤트 기반 구조

회로 상태 변경의 시작점은 다음 세 가지 이벤트이다.

1. ConnectPinToSocket
2. DisconnectPin
3. SwitchStateChanged

이 세 이벤트만 시스템 상태를 변경할 수 있다.

--------------------------------

2. ConnectPinToSocket(pin, socket)

설명
Pin이 특정 Socket에 연결될 때 호출된다.

동작

1. 연결 가능 여부 검사
2. 기존 연결이 있으면 먼저 Disconnect 처리
3. Pin.currentSocketId 갱신
4. Socket.connectedPinId 갱신
5. Evaluate() 호출

규칙

- Pin은 동시에 하나의 Socket에만 연결 가능하다.
- 연결 실패 시 상태는 저장하지 않는다.

--------------------------------

3. DisconnectPin(pin)

설명
Pin이 Socket에서 분리될 때 호출된다.

동작

1. 현재 연결된 Socket 확인
2. Socket.connectedPinId 제거
3. Pin.currentSocketId null 처리
4. Evaluate() 호출

--------------------------------

4. SwitchStateChanged(switchId, isOn)

설명
스위치 상태가 변경될 때 호출된다.

동작

1. Switch.isOn 값 변경
2. Evaluate() 호출

--------------------------------

5. Evaluate() 호출 규칙

Evaluate()는 다음 상황에서 반드시 실행된다.

- Pin 연결
- Pin 해제
- Switch 상태 변경

Evaluate()의 목적

- Node 그래프 생성
- 폐회로 판별
- LED 극성 판별
- 합선 여부 판별
- 회로 상태 반환

--------------------------------

6. 연결 제한 규칙

초기 버전에서는 다음 규칙을 사용한다.

1 Socket = 1 Pin

이유

- 디버깅 단순화
- 시스템 안정성
- 빠른 프로토타이핑

--------------------------------

7. 시스템 이벤트 흐름

XR Interaction

→ ConnectPinToSocket

또는

→ DisconnectPin

또는

→ SwitchStateChanged

↓

CircuitContext 상태 갱신

↓

Evaluate()

↓

회로 상태 결과

↓

LED / UI / Effect 반영