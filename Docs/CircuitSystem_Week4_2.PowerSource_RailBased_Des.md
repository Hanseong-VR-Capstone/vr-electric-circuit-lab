문서명: Week4_PowerSource_RailBased_Design_v1

목적
기존 CircuitEvaluator가 Battery 기반으로 회로 상태를 판정하던 구조를,
브레드보드 전원 레일 기반 구조로 변경한다.

--------------------------------

1. 변경 배경

기존 회로 판정 구조는 Battery_Pos / Battery_Neg를 기준으로 동작했다.

기존 흐름:
1. Battery 찾기
2. Battery_Pos / Battery_Neg의 nodeId 찾기
3. battery+ -> battery- 경로 판정
4. LED 극성 조건 판정
5. 상태 반환

하지만 현재 프로젝트는 배터리 오브젝트를 사용하지 않고,
브레드보드 자체가 전원에 연결되어 있는 설정을 사용한다.

따라서 회로 상태 판정 기준도
Battery 기준에서 Breadboard power rail 기준으로 변경해야 한다.

--------------------------------

2. 핵심 변경 방향

기존:
- 전원 기준 = Battery_Pos / Battery_Neg

변경:
- 전원 기준 = Breadboard Plus Rail / Minus Rail

즉 Evaluate는 더 이상 Battery를 찾지 않고,
브레드보드의 전원 레일 node를 기준으로 경로를 판정한다.

--------------------------------

3. 전원 레일 기준

초기 단순 규칙은 아래처럼 둔다.

전원 + 후보:
- TopPlus_0 ~ TopPlus_4
- BottomPlus_0 ~ BottomPlus_4

전원 - 후보:
- TopMinus_0 ~ TopMinus_4
- BottomMinus_0 ~ BottomMinus_4

판정 개념:
- plus rail 집합 중 하나에서
- minus rail 집합 중 하나로
유효 경로가 존재하면 회로가 연결된 것으로 본다.

--------------------------------

4. Evaluate 변경 목표

기존 Battery 관련 흐름을 제거하거나 우회하고,
아래 흐름으로 변경한다.

새 흐름:
1. power plus rail node 집합 준비
2. power minus rail node 집합 준비
3. 현재 연결 상태로 node graph 생성
4. plus rail -> minus rail 경로 존재 여부 확인
5. LED 극성 조건 확인
6. 상태 반환

--------------------------------

5. 상태 판정 기준

기본 상태 체계는 그대로 유지한다.

- Open
- LedOn
- Short
또는 현재 확장 구조를 쓰고 있다면
- Open
- LedOn
- Parallel
- Series
- Short

핵심 차이:
기존에는 battery path 존재 여부를 봤다면,
이제는 rail path 존재 여부를 본다.

--------------------------------

6. LED 판정 기준 변경

기존:
- battery+ -> anode
- cathode -> battery-

변경:
- plus rail -> anode
- cathode -> minus rail

즉 LED의 정상 점등 조건도
Battery 기준이 아니라 rail 기준으로 바뀐다.

--------------------------------

7. 구현 원칙

1. 기존 그래프 구조는 유지한다
2. Node = socket.nodeId 규칙 유지
3. Edge = wire / active switch 규칙 유지
4. Evaluate 전체를 갈아엎지 않는다
5. Battery 관련 helper를 rail 관련 helper로 대체하거나 분기 처리한다

--------------------------------

8. 필요한 신규 helper 방향

권장 helper 예시:

- GetPowerPlusNodes()
- GetPowerMinusNodes()
- HasPathFromAnyPlusRailToAnyMinusRail(...)
- IsValidLedPathFromRails(...)

핵심:
Battery lookup을 없애더라도
Evaluate는 여전히 orchestrator 역할만 유지해야 한다.

--------------------------------

9. 현재 단계에서 하지 않을 것

- 실제 전압/전류 정밀 계산 전체 변경
- UI 로직 변경
- 이펙트 로직 변경
- 브레드보드 레일 간 자동 점프 연결 같은 추가 규칙 확대

지금 단계는
"전원 기준을 battery -> rail로 교체"
하는 것이 핵심이다.

--------------------------------

10. 완료 기준

1. Battery가 없어도 Evaluate가 동작한다
2. plus rail -> minus rail 경로 기준으로 Open/LedOn 판정이 된다
3. LED 극성 판정이 rail 기준으로 동작한다
4. 기존 그래프 / service 구조는 유지된다

--------------------------------

11. 핵심 한 줄 요약

이번 수정의 핵심은
"회로 엔진의 전원 기준을 Battery에서 Breadboard power rail로 교체하는 것"이다.