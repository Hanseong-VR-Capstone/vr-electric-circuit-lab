CircuitDataStructure.md
VR Electric Circuit System
Day 3 Design – Core Data Structures

목적
회로 상태 판정을 위해 필요한 최소 데이터 구조를 정의한다.

이 문서의 목적은
- 이벤트가 들어왔을 때 무엇을 저장할지 정하고
- Evaluate()가 어떤 데이터를 읽을지 확정하는 것이다.

--------------------------------

1. Pin

Pin은 실제 연결의 시작 단위이다.

예:
- 점퍼 와이어 양 끝
- LED의 anode/cathode
- 배터리 +/-
- 스위치 단자

Pin이 가져야 하는 정보

- pinId
- ownerType
- ownerId
- currentSocketId

설명

pinId
각 Pin의 고유 식별자

ownerType
이 Pin이 어떤 부품에 속하는지
예: Wire, LED, Battery, Switch

ownerId
같은 종류 안에서 어느 부품인지 식별
예: LED1, Battery1, Wire3

currentSocketId
현재 연결된 Socket의 ID
연결이 없으면 null

규칙
- Pin은 동시에 하나의 Socket에만 연결 가능하다.

--------------------------------

2. Socket

Socket은 핀이 꽂히는 물리 위치이며,
동시에 어떤 전기적 Node에 속하는지 나타낸다.

Socket이 가져야 하는 정보

- socketId
- socketType
- nodeId
- connectedPinId

설명

socketId
각 Socket의 고유 식별자

socketType
BreadboardRow / PowerRailPlus / PowerRailMinus / ComponentTerminal

nodeId
이 Socket이 속한 전기적 Node 이름

connectedPinId
현재 꽂혀 있는 Pin의 ID
없으면 null

규칙
- 초기 버전에서는 1 Socket = 1 Pin 으로 제한한다.

--------------------------------

3. Wire

Wire는 두 개의 Pin을 가진 연결 부품이다.

Wire가 가져야 하는 정보

- wireId
- pinAId
- pinBId

설명

Wire는 전기적으로
pinA가 연결된 Node와 pinB가 연결된 Node를 이어주는 Edge 역할을 한다.

--------------------------------

4. Battery

Battery는 전원의 시작점이다.

Battery가 가져야 하는 정보

- batteryId
- positivePinId
- negativePinId
- voltage

설명

positivePinId
배터리 + 단자 Pin

negativePinId
배터리 - 단자 Pin

voltage
예: 1.5, 3.0, 9.0

--------------------------------

5. LED

LED는 극성이 있는 부품이다.

LED가 가져야 하는 정보

- ledId
- anodePinId
- cathodePinId

설명

anodePinId
긴 다리, + 방향

cathodePinId
짧은 다리, - 방향

Evaluate()에서
극성 방향이 맞는지 판단할 때 사용한다.

--------------------------------

6. Switch

Switch는 상태에 따라 연결이 생기거나 끊기는 부품이다.

Switch가 가져야 하는 정보

- switchId
- pinAId
- pinBId
- isOn

설명

isOn = true
pinA와 pinB가 연결된 것으로 본다.

isOn = false
연결되지 않은 것으로 본다.

--------------------------------

7. 전체 상태 저장 구조

시스템은 전체 회로 상태를 한 곳에 저장해야 한다.

예시 이름
- CircuitContext
- CircuitStateData
- CircuitBoardState

이 객체가 가져야 하는 정보

- pins
- sockets
- wires
- batteries
- leds
- switches

설명

pins
전체 Pin 목록

sockets
전체 Socket 목록

wires
전체 Wire 목록

batteries
전체 Battery 목록

leds
전체 LED 목록

switches
전체 Switch 목록

--------------------------------

8. Evaluate()가 이 구조를 사용하는 방식

Evaluate()는 다음 순서로 데이터를 읽는다.

1. 각 Pin이 현재 어떤 Socket에 연결되어 있는지 확인
2. Socket의 nodeId를 확인
3. Wire와 Switch를 통해 어떤 Node들이 이어지는지 그래프 생성
4. Battery + 에서 Battery - 로 경로가 있는지 확인
5. LED anode / cathode 방향이 올바른지 확인
6. 결과 상태 반환

--------------------------------

9. 3일차 핵심 규칙

1. Pin은 owner를 가진다.
2. Pin은 currentSocketId를 가진다.
3. Socket은 nodeId를 가진다.
4. Wire는 두 Pin을 가진다.
5. Battery는 + / - Pin을 가진다.
6. LED는 anode / cathode Pin을 가진다.
7. Switch는 on/off 상태를 가진다.
8. 전체 상태는 하나의 CircuitContext에 저장한다.