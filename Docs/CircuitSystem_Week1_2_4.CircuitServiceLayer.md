VR Electric Circuit System
Day 4 Design – Service Layer

목적
기존 데이터 계층(CircuitPin, CircuitSocket, CircuitContext 등) 위에
연결/해제/스위치 변경을 처리하는 서비스 계층을 설계한다.

이 계층의 역할은
- 입력 이벤트를 받아
- CircuitContext를 안전하게 갱신하고
- Evaluate()를 호출하는 것이다.

--------------------------------

1. 서비스 계층 개요

서비스 계층은 다음 세 가지 이벤트를 처리한다.

1. ConnectPinToSocket(pinId, socketId)
2. DisconnectPin(pinId)
3. SwitchStateChanged(switchId, isOn)

이 세 메서드는
CircuitContext를 수정할 수 있는 공식 진입점이다.

--------------------------------

2. 서비스 계층 책임

서비스 계층은 다음 책임을 가진다.

- id 기반 객체 조회
- 연결 가능 여부 검사
- 기존 연결 해제 처리
- Pin / Socket 상태 갱신
- Switch 상태 갱신
- Evaluate() 호출
- 실패 시 false 또는 실패 결과 반환

서비스 계층은 다음 책임을 가지지 않는다.

- UI 출력
- 사운드 재생
- 이펙트 재생
- XR 입력 처리

--------------------------------

3. ConnectPinToSocket(pinId, socketId)

설명
특정 Pin을 특정 Socket에 연결한다.

처리 순서

1. pinId로 Pin 조회
2. socketId로 Socket 조회
3. Pin 또는 Socket이 없으면 실패
4. Socket이 이미 다른 Pin으로 점유 중이면 실패
5. Pin이 기존에 다른 Socket에 연결되어 있으면 먼저 DisconnectPin(pinId) 실행
6. Pin.currentSocketId = socketId
7. Socket.connectedPinId = pinId
8. Evaluate() 호출
9. 성공 반환

규칙

- Pin은 동시에 하나의 Socket에만 연결 가능하다
- 1 Socket = 1 Pin 규칙을 유지한다
- 연결 실패 시 상태는 저장하지 않는다

--------------------------------

4. DisconnectPin(pinId)

설명
특정 Pin의 현재 연결을 해제한다.

처리 순서

1. pinId로 Pin 조회
2. Pin이 없으면 실패
3. currentSocketId가 null이면 실패 또는 무시 정책 선택
4. currentSocketId로 Socket 조회
5. Socket이 존재하면 Socket.connectedPinId = null
6. Pin.currentSocketId = null
7. Evaluate() 호출
8. 성공 반환

권장 정책

- Pin 없음: 실패
- 이미 연결 없음: false 반환
- Socket 참조가 깨져 있어도 Pin 쪽은 null로 정리

--------------------------------

5. SwitchStateChanged(switchId, isOn)

설명
스위치의 on/off 상태를 변경한다.

처리 순서

1. switchId로 Switch 조회
2. 없으면 실패
3. Switch.isOn 갱신
4. Evaluate() 호출
5. 성공 반환

규칙

- 스위치는 서비스 계층을 통해서만 상태 변경
- 직접 SetIsOn 호출을 외부에서 남용하지 않음

--------------------------------

6. Evaluate() 호출 규칙

Evaluate()는 반드시 아래 상황 이후 호출한다.

- 연결 성공 직후
- 연결 해제 직후
- 스위치 상태 변경 직후

초기 단계 규칙

- 서비스 계층은 Evaluate()를 "호출만" 한다
- 실제 회로 판정 알고리즘은 다음 단계에서 구현한다

--------------------------------

7. 반환 방식

초기 버전에서는 bool 반환을 사용한다.

예

- true = 상태 변경 성공
- false = 실패

이유

- 빠르게 구현 가능
- 디버깅 단순
- 나중에 필요하면 Result 객체로 확장 가능

--------------------------------

8. 예외 상황 처리 규칙

반드시 처리할 상황

- 없는 pinId
- 없는 socketId
- 없는 switchId
- 이미 점유된 socket
- 연결되지 않은 pin 해제
- Pin은 존재하지만 currentSocketId가 잘못된 경우

규칙

- 실패 상황은 조용히 무시하지 않는다
- 필요하면 Debug.LogWarning 사용
- 상태 무결성을 깨지 않는 방향으로 종료

--------------------------------

9. 권장 클래스 구조

예시 클래스 이름

- CircuitService
또는
- CircuitConnectionService

이 클래스는 CircuitContext 참조를 가진다.

또한 Evaluate 호출을 위해
나중에 solver/evaluator를 연결할 수 있는 구조를 고려한다.

초기 버전에서는 다음만 있어도 충분하다.

- context 참조
- ConnectPinToSocket()
- DisconnectPin()
- SwitchStateChanged()
- private Evaluate() 호출 지점

--------------------------------

10. 현재 단계의 핵심

지금 단계에서 중요한 것은
“정답 계산”이 아니라
“상태 변경이 항상 같은 규칙으로 일어나는 구조”를 만드는 것이다.

즉
입력 → 상태 갱신 → Evaluate 호출

이 흐름을 고정하는 것이 목적이다.