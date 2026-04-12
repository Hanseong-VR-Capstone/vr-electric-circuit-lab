문서명: CircuitEvaluator 경로 탐색 구조 정리 v1

목적
현재 회로 로직 코어 위에서 폐회로 판별의 기반이 되는 DFS/BFS 경로 탐색 구조를 명확히 분리한다.

현재 전제
- 기존 회로 로직 코어는 유지한다
- Node = socket.nodeId
- Edge = wire / 활성 switch
- Pin -> Socket -> NodeId
- Evaluate()는 현재 연결 상태를 읽고 회로 상태를 판정한다
- 기존 구조를 깨지 않고 보조 함수 수준으로 확장한다
:contentReference[oaicite:0]{index=0}
:contentReference[oaicite:1]{index=1}

왜 지금 이 단계가 필요한가
현재 Open / LedOn / Short 판정은 이미 동작한다.
하지만 이후 아래 기능을 안정적으로 확장하려면 공통 경로 탐색 기반이 먼저 정리되어야 한다.

- 폐회로 판별
- 극성 체크
- 직렬 / 병렬 판별
- 상태 전이 처리
- 단순 전압 분배 계산

즉 지금 단계의 핵심은
“판정 결과를 늘리는 것”이 아니라
“모든 판정이 공통으로 사용하는 그래프 탐색 기반을 정리하는 것”이다.

--------------------------------
[1] 현재 Evaluate 구조에서 유지할 것

현재 Evaluate의 큰 흐름은 유지한다.

1. Battery 찾기
2. Battery_Pos / Battery_Neg의 nodeId 찾기
3. BuildNodeGraph()
4. 경로 존재 여부 확인
5. LED 극성 조건 검사
6. 최종 상태 반환
:contentReference[oaicite:2]{index=2}
:contentReference[oaicite:3]{index=3}

즉 Evaluate 전체를 갈아엎지 않는다.
이번 단계에서는 “경로 탐색 부분”만 명시적으로 분리한다.

--------------------------------
[2] 이번 단계에서 추가할 핵심 보조 함수

1. TryGetNodeIdFromPin(string pinId, out string nodeId)
역할
- pinId -> currentSocketId -> socket.nodeId를 따라가며 nodeId를 찾는다

필요 이유
- 배터리 + / -
- LED anode / cathode
- switch / wire 양 끝
모두 동일한 방식으로 nodeId를 얻기 때문

2. BuildNodeGraph()
역할
- 현재 CircuitContext를 읽어
  Dictionary<string, List<string>> 형태의 양방향 그래프를 만든다

포함 대상
- Wire
- 활성화된 Switch

주의
- 연결 안 된 Pin은 그래프에 참여하지 않음
- nodeId를 못 얻은 Pin도 무시
:contentReference[oaicite:4]{index=4}

3. HasPath(string startNodeId, string targetNodeId)
역할
- startNodeId에서 targetNodeId로 도달 가능한지 검사
- BFS 또는 DFS 사용
- 초기 목적은 bool 반환

필요 이유
- Open 판정
- 일반 폐회로 판정
- LED 경로 판정
모두 경로 존재 여부를 공통으로 사용하기 때문
:contentReference[oaicite:5]{index=5}

--------------------------------
[3] 이번 단계에서의 경로 탐색 목표

이번 단계의 목표는 “최단 경로”가 아니다.
이번 단계의 목표는 아래 한 가지다.

- 두 node 사이에 경로가 존재하는가

즉 반환값은 단순 bool이면 충분하다.

예
- HasPath(batteryPlusNode, batteryMinusNode)
- HasPath(batteryPlusNode, ledAnodeNode)
- HasPath(ledCathodeNode, batteryMinusNode)

이렇게 공통 함수 하나로 여러 판정을 지원한다.

--------------------------------
[4] BFS와 DFS 중 선택

이번 단계에서는 BFS 또는 DFS 어느 쪽을 써도 된다.
다만 권장 방향은 BFS다.

이유
- 구현이 단순함
- 방문 처리 흐름이 명확함
- 이후 경로 자체를 저장하거나 단계별 확장하기 쉬움

즉 이번 단계 권장 구현은:
- Queue 사용
- visited 집합 사용
- 목표 node를 만나면 true 반환
- 끝까지 못 찾으면 false 반환

--------------------------------
[5] 함수 책임 분리 원칙

이번 단계에서 중요한 것은
Evaluate() 안에 탐색 코드를 길게 몰아넣지 않는 것이다.

권장 책임 분리

- TryGetNodeIdFromPin()
  -> pin 기준 nodeId 조회

- BuildNodeGraph()
  -> 현재 연결 상태를 그래프로 변환

- HasPath()
  -> 그래프 위에서 경로 존재 여부 검사

- Evaluate()
  -> 보조 함수들을 조합해 상태 판정

즉 Evaluate는 “오케스트레이션”만 하고,
세부 탐색은 보조 함수가 담당한다.

--------------------------------
[6] 그래프 생성 규칙 재정리

Wire
- pinAId의 nodeId 조회
- pinBId의 nodeId 조회
- 둘 다 유효하면 양방향 edge 추가

Switch
- isOn == true 일 때만 사용
- pinAId / pinBId nodeId 조회
- 둘 다 유효하면 양방향 edge 추가

LED
- 이번 단계에서는 일반 그래프 edge로 단순 처리하지 않는다
- LED는 기존처럼 극성 판정용 특수 부품으로 유지한다

이유
- 현재 문서 구조상 LED는 일반 wire와 동일 처리보다 별도 조건 검사 대상으로 유지하는 편이 안전하다
:contentReference[oaicite:6]{index=6}
:contentReference[oaicite:7]{index=7}

--------------------------------
[7] 이번 단계 완료 기준

이번 단계가 완료되면 아래가 가능해야 한다.

1. 배터리 + / - 사이 경로 존재 여부를 공통 함수로 확인 가능
2. LED anode / cathode 경로 판정을 공통 함수로 확인 가능
3. 기존 Open / LedOn / Short 판정에서 경로 탐색 코드가 분리됨
4. 이후 직렬 / 병렬 판별 확장 준비가 됨

즉 이번 단계는
“회로 해석 엔진의 탐색 기반 정리”
단계다.

--------------------------------
[8] 이번 단계에서는 하지 않을 것

- 직렬 / 병렬 판별 구현
- 전압 계산 구현
- 상태 전이 테이블 구현
- 브레드보드 / VR 오브젝트 연결

이것들은 다음 단계에서 한다.

--------------------------------
[9] 최종 정리

이번 단계의 핵심은 아래 한 줄이다.

“CircuitEvaluator 내부의 경로 탐색 로직을
재사용 가능한 공통 보조 함수로 분리한다.”

이렇게 하면 이후 기능 확장이 훨씬 쉬워진다.