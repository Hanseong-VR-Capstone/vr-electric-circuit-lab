VR Electric Circuit System
Day 5 Design – Evaluate Algorithm

목적
현재 CircuitContext에 저장된 연결 상태를 바탕으로
회로 상태를 판정하는 Evaluate() 알고리즘 구조를 정의한다.

현재 단계의 목표는
- 완전한 물리 시뮬레이터가 아니라
- 교육용 회로 상태 판정기를 만드는 것이다.

--------------------------------

1. Evaluate의 역할

Evaluate()는 현재 연결 상태를 읽고
회로 상태를 판정한다.

초기 버전에서 판정할 상태는 아래 4개다.

- Open
- Closed
- LedOn
- Short

설명

Open
배터리 + 에서 - 로 이어지는 유효한 폐회로가 없음

Closed
배터리 + 에서 - 로 이어지는 경로는 있으나
LED 점등 조건은 아직 만족하지 않음

LedOn
배터리 + 에서 LED anode를 거쳐
LED cathode를 지나
배터리 - 로 이어지는 올바른 경로가 존재함

Short
배터리 + 에서 - 로 LED 없이 직접 이어지는 위험 경로가 존재함

--------------------------------

2. 평가에 필요한 전제

Evaluate()는 아래 규칙을 전제로 한다.

1. Pin은 Socket에 연결된다
2. Socket은 NodeId를 가진다
3. Wire는 두 Pin을 잇는다
4. Switch는 isOn일 때만 두 Pin을 잇는다
5. Battery는 positivePinId / negativePinId를 가진다
6. LED는 anodePinId / cathodePinId를 가진다

즉 Evaluate()는
Pin → Socket → NodeId
를 따라가며 그래프를 만든다.

--------------------------------

3. 그래프 모델

그래프 기준

Node = nodeId
Edge = wire 또는 활성화된 switch가 연결하는 두 nodeId

추가 규칙

- 어떤 Pin이 Socket에 연결되지 않았으면 해당 Pin은 그래프에 참여하지 않는다
- Wire의 양 끝 Pin이 모두 Socket에 연결되어 있어야 Edge 생성 가능
- Switch도 pinA, pinB가 모두 연결되어 있고 isOn일 때만 Edge 생성 가능

--------------------------------

4. 기본 처리 순서

Evaluate()는 아래 순서로 동작한다.

1. Battery의 + nodeId 찾기
2. Battery의 - nodeId 찾기
3. 현재 연결 상태로 node graph 생성
4. Short 여부 먼저 검사
5. LED 점등 여부 검사
6. 일반 폐회로 여부 검사
7. 최종 CircuitState 반환

이 순서를 지키는 이유

- Short가 가장 우선순위가 높다
- LedOn은 Closed보다 더 구체적인 정상 상태다
- 마지막에 Open/Closed를 나누면 된다

--------------------------------

5. NodeId 찾기

배터리나 LED는 직접 nodeId를 가지지 않는다.
Pin이 연결된 Socket을 통해 nodeId를 얻는다.

예시

battery positivePinId
→ CircuitPin 찾기
→ currentSocketId 확인
→ CircuitSocket 찾기
→ socket.nodeId 획득

이 과정을 공통 함수로 분리할 수 있다.

예시 함수 이름

- TryGetNodeIdFromPin(string pinId, out string nodeId)

규칙

- Pin이 없거나
- Socket 연결이 없거나
- Socket 조회 실패 시

nodeId를 얻지 못한 것으로 처리한다.

--------------------------------

6. 그래프 생성 규칙

Wire에 대해

1. wire.pinAId의 nodeId 찾기
2. wire.pinBId의 nodeId 찾기
3. 둘 다 유효하면 양방향 edge 추가

Switch에 대해

1. switch.isOn 확인
2. switch.pinAId의 nodeId 찾기
3. switch.pinBId의 nodeId 찾기
4. 둘 다 유효하면 양방향 edge 추가

그래프는 Dictionary<string, List<string>> 또는 유사 구조로 표현 가능하다.

--------------------------------

7. Short 판정 규칙

초기 교육용 규칙

배터리 + nodeId 에서 배터리 - nodeId 로 가는 경로가 존재하는데
그 경로가 LED를 거치지 않으면 Short로 본다.

단순화 방식

- 먼저 일반 경로 존재 여부 확인
- 이후 LED를 포함한 유효 경로가 있는지 따로 확인
- 일반 경로는 있는데 LED 유효 경로가 없으면 Short 후보로 본다

주의
이 규칙은 교육용 단순화이며
실제 전자회로의 모든 경우를 완벽히 반영하지는 않는다.

--------------------------------

8. LED 점등 판정 규칙

LED 1개 기준 초기 규칙

LedOn 조건

1. batteryPlusNode 에서 ledAnodeNode 로 경로 존재
2. ledCathodeNode 에서 batteryMinusNode 로 경로 존재
3. ledAnodeNode 와 ledCathodeNode 가 유효하게 연결된 회로 맥락 안에 있어야 함

더 단순하게는

battery+ → led anode → led cathode → battery-

의 방향성이 성립한다고 판정하면 된다.

실제 구현에서는 LED를 특수 부품으로 보고
anode/cathode를 기준으로 경로 존재 여부를 판단한다.

--------------------------------

9. Closed 판정 규칙

Short도 아니고
LedOn도 아니지만

battery+ 와 battery- 사이에 어떤 경로든 존재하면 Closed

즉

- 배터리 기준 폐회로 존재
- 하지만 LED 정상 점등 조건은 아님

이 경우 Closed로 판정한다.

--------------------------------

10. Open 판정 규칙

아래 조건이면 Open

- battery+ nodeId를 찾을 수 없음
- battery- nodeId를 찾을 수 없음
- battery+ 에서 battery- 로 가는 경로가 없음

--------------------------------

11. 권장 enum

CircuitState enum 예시

- Open
- Closed
- LedOn
- Short

--------------------------------

12. 권장 보조 함수

Evaluate 구현 시 아래 보조 함수 구성을 권장한다.

- TryGetNodeIdFromPin(string pinId, out string nodeId)
- BuildNodeGraph()
- HasPath(string startNodeId, string targetNodeId)
- IsShortCircuit(...)
- IsLedOn(...)

핵심 규칙

- Evaluate() 하나에 모든 코드를 몰아넣지 않는다
- 판정용 보조 함수를 분리한다

--------------------------------

13. 현재 단계 핵심

지금은 “정답 계산기”보다
“판정 구조”를 만드는 단계다.

즉 중요한 것은

입력 상태
→ 그래프 생성
→ 규칙 순서대로 판정
→ CircuitState 반환

이 흐름을 고정하는 것이다.