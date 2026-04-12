문서명: Week4_VR_Breadboard_Bridge_Design_v2

목적
기존 그래프 기반 회로 엔진을 유지한 상태에서,
VR 브레드보드의 실제 hole 오브젝트들을 CircuitSocket / NodeId 구조에 연결한다.

--------------------------------

1. 현재 브레드보드 구성

브레드보드에는 Pin0 ~ Pin459까지의 개별 hole 오브젝트가 존재한다.

구조 특징:
- 각 Pin 오브젝트는 브레드보드의 실제 구멍 1개를 나타낸다
- 각 Pin 오브젝트마다 HoleTrigger가 하나씩 부착되어 있다
- HoleTrigger 내부에는 해당 hole의 holeIndex가 저장되어 있다
- 따라서 현재 브레드보드는 "1 hole = 1 GameObject = 1 HoleTrigger" 구조다

이 구조는 hole 단위 연결 처리의 기준점으로 사용할 수 있다.

--------------------------------

2. 이번 단계 핵심 목표

이번 단계의 목적은
VR 오브젝트가 브레드보드 hole에 닿았을 때
기존 Circuit 엔진의 pinId / socketId 기반 연결 구조로 안전하게 전달하는 것이다.

즉 다음 흐름을 완성한다.

VR PartPin
-> breadboard hole 감지
-> holeIndex 확인
-> socketId / nodeId 해석
-> CircuitConnectionService 호출
-> Evaluate()
-> 상태 반영

--------------------------------

3. 설계 원칙

1. 기존 회로 엔진 구조는 변경하지 않는다
2. Node 기반 계산 구조를 유지한다
3. Evaluate 로직은 수정하지 않는다
4. Service Layer를 통해서만 연결/해제를 반영한다
5. UI / 이펙트 / XR 세부 동작은 bridge 계층에 넣지 않는다

--------------------------------

4. HoleTrigger 사용 원칙

HoleTrigger는 현재 모든 hole 오브젝트에 이미 부착되어 있으므로,
초기 구현에서는 이를 활용할 수 있다.

하지만 이번 단계에서 중요한 것은
"HoleTrigger를 반드시 그대로 써야 한다"가 아니라
"holeIndex를 안정적으로 얻고 연결 이벤트를 Service Layer로 전달하는 것"이다.

따라서 구현 방향은 두 가지 모두 허용한다.

A안
- 기존 HoleTrigger를 보조 입력원으로 사용
- 별도 bridge 스크립트가 holeIndex를 읽어 Service 호출

B안
- 필요 시 새 감지 스크립트를 작성
- holeIndex를 직접 참조하거나 동일 GameObject의 HoleTrigger에서 읽음
- 기존 HoleTrigger는 디버그용/보조용으로 유지 가능

즉 핵심은 HoleTrigger 재사용 여부가 아니라
holeIndex 기반 매핑과 Service 연결 완성이다.

--------------------------------

5. 필요한 신규 계층

이번 단계에서 필요한 핵심 구성은 아래 3개다.

1) BreadboardNodeMapper
역할:
- holeIndex -> socketId 변환
- holeIndex -> nodeId 변환
- holeIndex -> socketType 반환

2) BreadboardSocketBootstrap
역할:
- 시작 시 Pin0 ~ Pin459 전체를 CircuitSocket으로 등록
- 모든 socketId / nodeId / socketType을 CircuitContext에 넣음

3) BreadboardHoleBridge
역할:
- VR PartPin이 hole에 연결/해제될 때
  CircuitConnectionService.ConnectPinToSocket / DisconnectPin 호출

--------------------------------

6. BreadboardNodeMapper 규칙

[상단 - 레일]
0~24
- 0~4 -> TopMinus_0
- 5~9 -> TopMinus_1
- 10~14 -> TopMinus_2
- 15~19 -> TopMinus_3
- 20~24 -> TopMinus_4

[상단 + 레일]
25~49
- 25~29 -> TopPlus_0
- 30~34 -> TopPlus_1
- 35~39 -> TopPlus_2
- 40~44 -> TopPlus_3
- 45~49 -> TopPlus_4

[상단 중앙]
50~229
- 50~54 -> UpperRow_0
- 55~59 -> UpperRow_1
- ...
- 225~229 -> UpperRow_35

[하단 중앙]
230~409
- 230~234 -> LowerRow_0
- 235~239 -> LowerRow_1
- ...
- 405~409 -> LowerRow_35

[하단 - 레일]
410~434
- 410~414 -> BottomMinus_0
- 415~419 -> BottomMinus_1
- 420~424 -> BottomMinus_2
- 425~429 -> BottomMinus_3
- 430~434 -> BottomMinus_4

[하단 + 레일]
435~459
- 435~439 -> BottomPlus_0
- 440~444 -> BottomPlus_1
- 445~449 -> BottomPlus_2
- 450~454 -> BottomPlus_3
- 455~459 -> BottomPlus_4

socketId 규칙은 hole 단위 고유값을 사용한다.
예:
- BB_H0
- BB_H1
- ...
- BB_H459

--------------------------------

7. BreadboardSocketBootstrap 설계

중요:
CircuitConnectionService는 socketId로 CircuitSocket을 조회한다.

따라서 시작 시점에
BB_H0 ~ BB_H459 전부가 CircuitContext.sockets 안에 등록되어 있어야 한다.

Bootstrap의 역할:
1. 0~459 holeIndex 순회
2. socketId 생성
3. nodeId 생성
4. socketType 결정
5. CircuitSocket 생성
6. CircuitContext에 등록

즉 bridge는 socketId를 넘기기만 하고,
실제 전기적 의미는 미리 등록된 CircuitSocket이 가진다.

--------------------------------

8. BreadboardHoleBridge 설계

역할:
실제 hole 감지 이벤트를 CircuitConnectionService로 전달한다.

처리 원칙:

연결 시
1. 들어온 collider에서 PartPin 확인
2. 해당 PartPin의 circuit pinId 해석
3. 현재 hole의 holeIndex 확인
4. holeIndex -> socketId 변환
5. ConnectPinToSocket(pinId, socketId) 호출

해제 시
1. 나가는 collider에서 PartPin 확인
2. circuit pinId 해석
3. DisconnectPin(pinId) 호출

주의:
- hole 점유 상태를 무시하면 안 된다
- 1 Socket = 1 Pin 규칙을 유지해야 한다
- 실제 현재 hole에 꽂힌 pin과 일치할 때만 해제 처리해야 한다

--------------------------------

9. pinId 해석 원칙

PartPin -> pinId 변환은 임시 문자열 조합에만 의존하면 위험하다.

가능하면 아래 중 하나를 사용한다.

권장 1
- PartPin이 직접 circuitPinId를 가짐

권장 2
- 별도 PartPinAdapter가 circuitPinId를 제공

임시 가능
- parentPart.name + "_" + pinRole

하지만 임시 규칙은
GameObject 이름 변경, Clone 이름, 수동 수정 등에 취약하므로
최종 구조로 오래 유지하지 않는다.

--------------------------------

10. 이번 단계 완료 기준

아래가 모두 되면 4주차 연결 단계 완료로 본다.

1. 모든 hole이 socketId / nodeId로 해석 가능
2. CircuitContext에 460개 hole socket이 등록됨
3. VR에서 pin을 꽂으면 ConnectPinToSocket 호출
4. pin을 빼면 DisconnectPin 호출
5. LED / 배터리 / 점퍼와이어 / 스위치가 브레드보드 hole 기반으로 회로 엔진과 연결됨
6. Evaluate 결과가 VR 입력에 따라 즉시 바뀜

--------------------------------

11. 핵심 한 줄 요약

이번 단계의 핵심은
"브레드보드 hole 오브젝트를 CircuitSocket으로 등록하고,
VR pin 연결 이벤트를 기존 CircuitConnectionService에 안전하게 전달하는 것"이다.