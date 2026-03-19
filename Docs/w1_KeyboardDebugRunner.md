목적
VR 없이 키보드 입력만으로 현재 회로 시스템을 임시 테스트한다.

방식
- 숫자 키를 누르면 미리 정의한 시나리오를 적용
- 모든 tracked pin을 먼저 해제
- 시나리오에 등록된 pinId -> socketId 연결을 순서대로 수행
- 필요 시 switch 상태도 적용
- 마지막에 현재 회로 상태와 pin/socket 요약을 로그로 출력

주의
현재 규칙은 1 Socket = 1 Pin 이다.
따라서 같은 전기적 node를 공유하려면
서로 다른 socketId가 같은 nodeId를 가져야 한다.

예
- RailPlus_A -> nodeId = RailPlus
- RailPlus_B -> nodeId = RailPlus
- RailMinus_A -> nodeId = RailMinus
- RailMinus_B -> nodeId = RailMinus