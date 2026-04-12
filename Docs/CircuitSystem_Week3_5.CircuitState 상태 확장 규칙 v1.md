문서명: CircuitState 상태 확장 규칙 v1

목적
현재 CircuitEvaluator의 기본 상태(Open / LedOn / Short) 위에
회로 구조 정보(직렬 / 병렬)를 해석 가능한 확장 상태 체계로 정리한다.

이 문서는:
- 기존 상태 판정 구조를 깨지 않고
- Series / Parallel 정보를 상태 계층에 연결할 수 있게 하며
- 이후 전압 분배 / 디버그 출력 / UI 설명의 기준을 제공한다

--------------------------------
[1] 현재 전제

현재 CircuitEvaluator는 이미 다음 기능을 가진다.

- 폐회로 판별
- LED 극성 체크
- 상태 전이 구조 정리
- 직렬 회로 판별 helper
- 병렬 회로 판별 helper
- 단순 전압 분배 helper

즉 지금 단계는
“새 판정 기능 추가”가 아니라
“이미 준비된 판정 결과를 상태 체계에 연결하는 단계”다.
:contentReference[oaicite:0]{index=0}
:contentReference[oaicite:1]{index=1}

--------------------------------
[2] 상태 확장 방향

현재 CircuitState는 다음 3개를 사용한다.

- Open
- LedOn
- Short

이번 단계에서는 기존 상태를 바로 삭제하지 않는다.
대신 아래 두 방향 중 하나를 준비한다.

방향 A
기존 CircuitState enum 확장
- Open
- LedOn
- Short
- Series
- Parallel

방향 B
기존 CircuitState는 유지하고
별도의 구조 정보 enum 또는 helper 반환값을 둔다

현재 프로젝트 흐름상 이번 단계 추천은 방향 A다.
이유:
- 교육용 설명에 직접 사용하기 쉽다
- 디버그 출력과 UI 연결이 단순하다
- 이후 전압 분배 결과와 묶기 쉽다

--------------------------------
[3] 상태 의미 재정의

Open
- battery+ 와 battery- 사이 경로 없음

LedOn
- battery+ 와 battery- 경로 존재
- LED 극성 조건 만족

Short
- battery+ 와 battery- 경로 존재
- 정상 LED 조건 불만족
- 직렬/병렬 정상 구조 설명보다 “잘못된 상태” 우선

Series
- battery+ 와 battery- 경로 존재
- LED 점등 상태는 아니지만
- 회로 구조가 직렬 규칙을 만족

Parallel
- battery+ 와 battery- 경로 존재
- LED 점등 상태는 아니지만
- 회로 구조가 병렬 규칙을 만족

중요
- LedOn은 여전히 정상 동작 우선 상태다
- Short는 잘못된 회로 상태 우선이다
- Series / Parallel는 “비 LED / 비 Short 구조 설명 상태”에 가깝다

--------------------------------
[4] 상태 판정 우선순위

이번 단계의 핵심은 우선순위 고정이다.

추천 우선순위

1. battery 정보 또는 terminal node 해석 실패 -> Open
2. battery+ -> battery- 경로 없음 -> Open
3. 유효 LED 경로 존재 -> LedOn
4. 구조가 병렬 -> Parallel
5. 구조가 직렬 -> Series
6. 그 외 경로 존재 -> Short

즉 최종 우선순위는:

Open
-> LedOn
-> Parallel
-> Series
-> Short

주의
- Parallel을 Series보다 먼저 검사한다
- 이유: 분기 구조는 더 구체적인 구조 정보이기 때문
- Series는 “분기 없는 연결 구조”일 때만 의미 있다

--------------------------------
[5] 왜 Short를 마지막에 두는가

이전에는 “LED 아니면 Short”였다.
하지만 이제는 구조 helper가 이미 준비되어 있다.

따라서 다음처럼 바꾼다.

기존:
- no path -> Open
- LED valid -> LedOn
- else -> Short

확장:
- no path -> Open
- LED valid -> LedOn
- parallel 구조 -> Parallel
- series 구조 -> Series
- else -> Short

즉 Short는
“구조적으로도 설명이 잘 안 되는 남은 비정상 상태”
로 뒤로 밀린다.

--------------------------------
[6] 직렬 / 병렬 helper 사용 규칙

기존 helper를 그대로 사용한다.

- IsSeriesCircuit(startNode, endNode, graph)
- IsParallelCircuit(startNode, endNode, graph)

이번 단계에서는 이 helper를
처음으로 최종 상태 반환에 실제 사용한다.

--------------------------------
[7] 전압 helper와의 관계

이번 단계에서는 전압 계산 자체를 Evaluate에 직접 넣지 않는다.

하지만 상태가
- Series
- Parallel
로 구분되면

이후 보조 출력에서:
- Series -> CalculateSeriesVoltagePerLoad()
- Parallel -> CalculateParallelBranchVoltage()

를 연결하기 쉬워진다.

즉 이번 단계는 전압 계산을 쓰는 단계가 아니라
전압 계산을 “쓸 수 있게 되는 단계”다.

--------------------------------
[8] 구현 방향

CircuitEvaluator에서 아래 부분을 수정한다.

1. CircuitState enum 확장
- Series
- Parallel 추가

2. EvaluateStateFromResolvedBattery(...) 수정
현재:
- Open
- LedOn
- Short

변경 후:
- Open
- LedOn
- Parallel
- Series
- Short

3. DecideNonLedCircuitState(...) 수정
현재:
- 무조건 Short 반환

변경 후:
- IsParallelCircuit(...) -> Parallel
- else if IsSeriesCircuit(...) -> Series
- else -> Short

즉 비 LED 상태 결정을
별도 decision point에서 계속 유지한다.

--------------------------------
[9] 이번 단계에서 하지 않을 것

- Closed 상태 추가
- SeriesLedOn / ParallelLedOn 같은 세부 상태 추가
- 전압 계산 결과를 CircuitState에 직접 포함
- UI / 디버그 문자열 생성

--------------------------------
[10] 핵심 요약

기존:
- Open
- LedOn
- Short

확장:
- Open
- LedOn
- Parallel
- Series
- Short

판정 우선순위:
1. Open
2. LedOn
3. Parallel
4. Series
5. Short

이번 단계 목적:
“준비된 구조 판별 결과를 최종 상태 반환에 연결”