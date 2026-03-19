BreadboardNodeDesign.md
VR Electric Circuit System
Day 2 Design – Socket & Node Structure

목적
브레드보드와 부품의 전기적 연결을 단순한 Node 기반 그래프로 표현하기 위한 규칙을 정의한다.

이 구조는 이후 Evaluate() 알고리즘이 회로 상태를 판정하는 기반이 된다.

--------------------------------

1. Socket 개념

Socket은 “핀을 꽂는 물리 위치”이며 동시에 전기적으로 어떤 Node에 속하는지를 나타낸다.

각 Socket은 다음 정보를 가진다.

- socketId
- socketType
- nodeId

--------------------------------

2. SocketType 정의

Socket은 아래 4가지 타입 중 하나를 가진다.

BreadboardRow  
브레드보드 중앙 영역에 위치한 구멍

PowerRailPlus  
브레드보드 + 전원 레일

PowerRailMinus  
브레드보드 - 전원 레일

ComponentTerminal  
LED, 배터리, 스위치 등의 부품 단자

--------------------------------

3. Breadboard Node 구조

브레드보드 중앙 영역은 “Row + Side” 기준으로 Node를 나눈다.

같은 row, 같은 side의 구멍들은 모두 같은 Node에 속한다.

예시

Row1_L
Row1_R
Row2_L
Row2_R

설명

Row1_L  
→ 왼쪽 1번 줄의 모든 구멍

Row1_R  
→ 오른쪽 1번 줄의 모든 구멍

주의

브레드보드 중앙 홈 때문에

Row1_L 과 Row1_R 은 기본적으로 연결되지 않는다.

--------------------------------

4. Power Rail 구조

전원 레일은 단순화하여 두 개만 사용한다.

RailPlus  
RailMinus

같은 레일의 모든 Socket은 같은 NodeId를 가진다.

--------------------------------

5. Component Terminal 규칙

부품 단자는 고유 식별자를 가진다.

예시

Battery_Pos
Battery_Neg

LED1_Anode
LED1_Cathode

Switch1_A
Switch1_B

이 Node들은 브레드보드 또는 와이어 연결을 통해 다른 Node와 연결된다.

--------------------------------

6. NodeId 역할

NodeId는 “전기적으로 동일한 위치”를 나타낸다.

예

Row3_L 에 연결된 모든 Socket은 같은 전기적 위치로 간주된다.

Evaluate() 알고리즘은 NodeId를 기준으로 그래프를 구성한다.

--------------------------------

7. 시스템 동작 개념

연결 이벤트 발생

Pin → Socket 연결

Socket → NodeId 확인

Node 기반 그래프 생성

Evaluate() 실행

회로 상태 판정