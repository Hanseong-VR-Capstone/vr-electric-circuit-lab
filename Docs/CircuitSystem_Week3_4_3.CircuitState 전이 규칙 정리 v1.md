문서명: CircuitState 전이 규칙 정리 v1

목적
현재 그래프 기반 회로 판정 결과를
명시적인 상태 전이 규칙으로 정리한다.

이 문서는:
- Evaluate()의 상태 반환 기준을 고정하고
- 연결/해제/스위치 변경 이후 상태가 어떻게 결정되는지 정리하며
- 이후 Series / Parallel / Voltage 확장의 기반을 만든다

--------------------------------
[1] 현재 전제

현재 CircuitEvaluator는 다음 기반을 이미 가진다.

- BuildNodeGraph()
- HasPath()
- IsValidLedPath()
- IsSeriesCircuit()
- IsParallelCircuit()

현재 핵심 판정 재료:
1. battery+ -> battery- 경로 존재 여부
2. LED 극성 경로 유효 여부
3. 분기 여부 / 경로 수 기반 구조 판별 준비 완료

즉 지금 단계는
“판정 재료 부족” 단계가 아니라
“판정 결과를 상태 체계로 정리하는 단계”다.

--------------------------------
[2] 현재 유지할 기본 상태

현재 기본 상태는 다음 3개를 유지한다.

- Open
- LedOn
- Short

설명

Open
- battery+ 와 battery- 사이 유효 경로가 없음

LedOn
- battery+ 와 battery- 사이 경로가 존재하고
- LED 극성 조건이 만족됨

Short
- battery+ 와 battery- 사이 경로가 존재하지만
- 현재 정상 LED 조건은 만족하지 않음

주의
- Closed는 아직 실제 반환 상태로 도입하지 않는다
- Series / Parallel도 아직 최종 CircuitState로 넣지 않는다
- 이번 단계에서는 “상태 전이 구조 정리”가 목적이다

--------------------------------
[3] 상태 전이 발생 시점

상태 전이는 아래 이벤트 직후 Evaluate()를 다시 수행하면서 발생한다.

- ConnectPinToSocket
- DisconnectPin
- SwitchStateChanged

즉 별도 상태 전이 함수를 두기보다,
이벤트 -> Evaluate() -> 새 상태 결정
흐름을 명확히 하는 것이 목적이다.
:contentReference[oaicite:0]{index=0}

--------------------------------
[4] 상태 전이 규칙

가장 기본 전이

1. Open -> LedOn
조건:
- 새 연결 또는 스위치 on 이후
- battery+ -> battery- 경로 생성
- LED 극성 조건 만족

2. Open -> Short
조건:
- 새 연결 또는 스위치 on 이후
- battery+ -> battery- 경로 생성
- LED 극성 조건 불만족

3. LedOn -> Open
조건:
- 회로가 끊김
- battery+ -> battery- 경로 소실
예:
- wire 제거
- switch off
- LED 핀 분리

4. Short -> Open
조건:
- 합선 경로 소실
- battery+ -> battery- 경로 자체가 사라짐

5. Short -> LedOn
조건:
- 기존 잘못된 경로 상태에서
- 연결이 수정되어
- 정상 LED 극성 경로가 형성됨

6. LedOn -> Short
조건:
- 경로는 남아 있지만
- LED 방향 또는 연결이 잘못되어
- 정상 LED 조건이 깨짐

--------------------------------
[5] 상태 전이 우선순위

Evaluate()의 상태 판정 우선순위는 아래처럼 유지한다.

1. battery 또는 battery node 조회 실패 -> Open
2. battery+ -> battery- 경로 없음 -> Open
3. LED 정상 조건 만족 -> LedOn
4. 그 외 battery 경로 존재 -> Short

즉 판정 우선순위는

Open
-> LedOn
-> Short

순서가 아니라

Open 우선 배제
그 다음 정상 LED 판정
마지막 나머지 Short

방식이다.

--------------------------------
[6] 상태 전이와 구조 판별의 관계

현재 준비된 구조 판별 함수는:

- IsSeriesCircuit()
- IsParallelCircuit()

하지만 이번 단계에서는 이 값을
최종 CircuitState에 직접 넣지 않는다.

이유
- 현재 목표는 기존 3상태 체계를 안정화하는 것
- 구조 판별은 이후 설명/전압 계산/세부 상태 확장에 사용 예정

즉 지금 단계에서는:
- 상태 전이 기본 체계 확정
- 구조 판별은 보조 정보로만 유지

--------------------------------
[7] Evaluate() 설계 원칙

이번 단계에서 Evaluate()는 다음 책임만 가진다.

1. 현재 연결 상태를 읽는다
2. 회로 상태를 판정한다
3. CircuitState 하나를 반환한다

즉:
- 이전 상태를 직접 저장하지 않는다
- 상태 히스토리를 관리하지 않는다
- 이벤트 처리 계층과 역할을 섞지 않는다

현재 상태 전이란
“이전 상태 + 이벤트로 직접 상태 머신 구현”
이 아니라

“이벤트 후 재평가 결과가 새 상태가 되는 구조”
로 본다

--------------------------------
[8] 현재 단계 완료 기준

이번 단계 완료 기준은 아래와 같다.

1. Evaluate()의 최종 상태 반환 기준이 문서로 고정됨
2. Open / LedOn / Short 간 전이 규칙이 명확해짐
3. Connect / Disconnect / Switch 변화 후 어떤 상태가 나오는지 설명 가능
4. 이후 Series / Parallel / Voltage 단계로 넘어갈 준비 완료

--------------------------------
[9] 이번 단계에서는 하지 않을 것

- Closed 상태 실제 추가
- Series / Parallel를 CircuitState enum에 반영
- 전압 계산 결과 반영
- UI / 이펙트 상태 머신 구현

--------------------------------
[10] 핵심 요약

현재 상태 전이는
“상태 머신 내부 저장” 방식이 아니라
“이벤트 후 Evaluate() 재실행 결과가 새 상태가 되는 방식”이다.

기본 상태는 유지:
- Open
- LedOn
- Short

기본 전이:
- Open <-> LedOn
- Open <-> Short
- LedOn <-> Short