VR Electric Circuit System
Week 1-2 Final Summary (Role A – Circuit Logic)

목적
VR 전기 회로 교육 프로젝트에서 A 역할(회로 로직)의 1~2주차 범위를 최종 정리한다.

이 문서는 다음을 포함한다.
- Node/Edge 기반 그래프 구조
- 핀 단위 데이터 구조
- 회로 상태 구조
- LED 조건 정의
- Breadboard 전기적 Node 모델
- 현재 구현 및 검증 결과

==================================================
1. 현재 1~2주차 범위에서 완료된 항목
==================================================

1) Node/Edge 기반 그래프 구조 설계
완료

현재 회로는 그래프 구조로 해석된다.

- Node = socket.nodeId
- Edge = wire / switch / LED

즉 실제 계산 기준은 부품 자체가 아니라
Socket이 속한 nodeId를 기준으로 한다.

현재 구현된 핵심 구조:
- BuildNodeGraph()
- AddWireEdges()
- AddSwitchEdges()
- AddLedEdges()
- HasPath()

이로써 회로 연결 상태를 그래프로 판정할 수 있다.

--------------------------------------------------

2) 핀 단위 데이터 구조 정의
완료

현재 정의된 데이터 구조:
- CircuitPin
- CircuitSocket
- CircuitWire
- CircuitBattery
- CircuitLed
- CircuitSwitch
- CircuitContext

핵심 관계:
Pin -> Socket -> NodeId

CircuitPin은 다음 정보를 가진다.
- pinId
- ownerType
- ownerId
- currentSocketId

CircuitSocket은 다음 정보를 가진다.
- socketId
- socketType
- nodeId
- connectedPinId

즉 회로의 실제 연결 단위는 Pin이며,
전기적 위치 판정 단위는 NodeId이다.

--------------------------------------------------

3) 회로 상태 구조 설계
거의 완료

현재 CircuitState:
- Open
- Closed
- LedOn
- Short

현재 실제 1차 평가기에서 주로 사용하는 상태:
- Open
- LedOn
- Short

Closed는 현재 예약 상태이며,
추후 더 세분화된 규칙이 필요할 때 확장 가능하다.

현재 상태 변화 흐름:
입력 이벤트
-> Connect / Disconnect / SwitchStateChanged
-> Evaluate()
-> CurrentState 갱신

즉 교육용 회로 상태 머신의 기본 구조는 이미 완성된 상태다.

--------------------------------------------------

4) 전압 전달 기본 구조 설계
미구현

현재 구현은 전압/전류/저항의 실제 수치 계산이 아니라
회로 구조와 경로 존재 여부를 판정하는 구조이다.

즉 현재 시스템은
- 전압 시뮬레이터
보다는
- 회로 구조 판정기
에 가깝다.

현재 단계에서는 교육용 기초 회로 구조 이해가 목적이므로
이 상태로도 충분하다.

--------------------------------------------------

5) LED 조건 정의
완료

현재 LED 정상 조건은 다음과 같다.

battery+ -> LED anode
LED cathode -> battery-

즉 방향성이 맞는 경우에만 LedOn이 된다.

LED는 그래프 상 edge로 참여하며,
극성 판정은 별도로 수행한다.

==================================================
2. 현재 시스템 계층 구조
==================================================

현재 구조는 4계층으로 나뉜다.

1) Data Layer
- CircuitContext
- CircuitPin
- CircuitSocket
- CircuitWire
- CircuitBattery
- CircuitLed
- CircuitSwitch

역할:
회로 상태 저장

--------------------------------------------------

2) Service Layer
- CircuitConnectionService

역할:
- ConnectPinToSocket
- DisconnectPin
- SwitchStateChanged
- Evaluate 호출

--------------------------------------------------

3) Evaluation Layer
- CircuitEvaluator
- CircuitState

역할:
현재 회로 상태 판정

--------------------------------------------------

4) Debug Layer
- CircuitDebugValidator
- CircuitQuickTestBootstrap
- CircuitKeyboardDebugRunner

역할:
키보드 기반 임시 테스트
상태 요약 출력
연결 일관성 검사

==================================================
3. Breadboard 전기적 모델
==================================================

중요:
실제 브레드보드 에셋이 없어도
전기적 모델은 먼저 설계할 수 있다.

현재 A가 정한 것은
“실제 오브젝트 배치”가 아니라
“어떤 소켓들이 같은 전기적 위치인가”에 대한 규칙이다.

--------------------------------------------------
3-1. Breadboard 영역 구분
--------------------------------------------------

브레드보드는 크게 3개 영역으로 해석한다.

1) 중앙부
예:
- Row1_L
- Row1_R
- Row2_L
- Row2_R
- Row3_L
- Row3_R

규칙:
- 같은 row + 같은 side = 같은 Node
- 왼쪽과 오른쪽은 기본적으로 분리됨
- 예: Row1_L != Row1_R

2) 전원 레일
- RailPlus
- RailMinus

규칙:
- 같은 레일 전체는 하나의 Node로 봄

3) 부품 단자
예:
- Battery_Pos
- Battery_Neg
- LED1_Anode
- LED1_Cathode
- Switch1_A
- Switch1_B

규칙:
- 각 부품 핀은 독립 단자이지만
  연결되면 해당 Socket의 nodeId에 편입된다

--------------------------------------------------
3-2. 핵심 규칙
--------------------------------------------------

규칙 1
같은 nodeId = 같은 전기적 위치

규칙 2
socketId는 달라도 nodeId가 같으면 전기적으로 연결된 것으로 본다

예:
- RailPlus_A -> nodeId = RailPlus
- RailPlus_B -> nodeId = RailPlus

즉 두 socketId는 다르지만 같은 전기적 위치이다.

규칙 3
현재 시스템은 1 Socket = 1 Pin 규칙을 사용한다.

따라서 하나의 전기적 위치에 여러 핀을 연결하려면
같은 nodeId를 공유하는 서로 다른 socketId를 준비해야 한다.

규칙 4
그래프 연결은 socketId가 아니라 nodeId 기준으로 판단된다.

==================================================
4. Evaluate 구조
==================================================

현재 Evaluate 흐름은 다음과 같다.

1. Battery 찾기
2. Battery_Pos / Battery_Neg의 nodeId 찾기
3. BuildNodeGraph()
4. battery+ 에서 battery- 경로 존재 여부 확인
5. LED 극성 조건 검사
6. 상태 반환

--------------------------------------------------
4-1. 상태 의미
--------------------------------------------------

Open
- battery+ 와 battery- 사이 경로 없음

LedOn
- battery+ -> anode
- cathode -> battery-
- LED 정상 방향 조건 만족

Short
- battery+ 와 battery- 사이 경로는 존재
- 하지만 LED 정상 조건은 아님

Closed
- 현재 예약 상태
- 추후 세분화 시 사용 가능

==================================================
5. 현재까지의 실제 테스트 결과
==================================================

키보드 기반 임시 테스트 수행 완료

테스트 시나리오:
1. Open
2. LedOn
3. Short

결과:
- Open -> 정상
- LedOn -> 정상
- Short -> 정상

추가 검증:
- Pin-Socket 연결 상태 일관성 검사 -> 정상

즉 현재 회로 로직 코어는
교육용 1차 프로토타입 기준으로 정상 동작함이 확인되었다.

==================================================
6. 현재 상태 평가
==================================================

A 역할 기준 1~2주차 목표 중 현재 상태는 다음과 같다.

완료:
- Node/Edge 기반 그래프 구조 설계
- 핀 단위 데이터 구조 정의
- 회로 상태 구조 설계
- LED 조건 정의
- Breadboard 전기적 Node 규칙 정의
- 키보드 기반 임시 로직 테스트

미구현:
- 전압 전달 기본 구조 설계
- 실제 전압/전류/저항 계산
- 실제 VR 오브젝트와 연결

즉 현재 상태는

“회로 엔진 코어와 전기적 규칙 설계는 완료”
상태다.

==================================================
7. 실제 브레드보드 에셋과 연결될 때의 의미
==================================================

“VR 오브젝트 -> NodeId 연결”이란 다음을 의미한다.

실제 브레드보드 에셋의 각 구멍/소켓 오브젝트에 대해
- socketId를 부여하고
- 그 socket이 어떤 nodeId에 속하는지 지정하는 것

예:
- BB_Row1_L_1 -> socketId = BB_Row1_L_1, nodeId = Row1_L
- BB_Row1_L_2 -> socketId = BB_Row1_L_2, nodeId = Row1_L
- BB_Row1_L_3 -> socketId = BB_Row1_L_3, nodeId = Row1_L

즉 실제 오브젝트는 다르지만
같은 전기적 row이면 같은 nodeId를 공유한다.

정리하면:

에셋 준비 전 A 역할
- 전기적 규칙 설계
- nodeId 구조 설계
- 회로 로직 검증

에셋 준비 후 A 역할
- 실제 소켓 오브젝트를 nodeId에 매핑
- XR 연결 이벤트를 ConnectPinToSocket과 연결
- 실제 LED 점등 이펙트와 CurrentState를 연결

==================================================
8. 결론
==================================================

현재 A 역할의 1~2주차 범위는 사실상 완료 상태다.

정확히 완료된 핵심:
- 그래프 기반 회로 모델
- 핀/소켓 데이터 구조
- 연결 서비스 계층
- 평가 계층
- 디버그 검증 계층
- Breadboard 전기적 Node 규칙
- 키보드 기반 임시 테스트 통과

남은 것은
“실제 VR 오브젝트와 이 구조를 연결하는 단계”이며,
그건 브레드보드 에셋과 XR 인터랙션 준비 후 진행하면 된다.