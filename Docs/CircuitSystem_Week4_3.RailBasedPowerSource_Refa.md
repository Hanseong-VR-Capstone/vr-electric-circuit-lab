문서명: Week4_RailBasedPowerSource_Refactor_v1

목적
기존 회로 엔진에서 Battery 기반 전원 판정과 Battery 기반 helper를 제거하고,
브레드보드 power rail 기반 전원 구조로 통일한다.

--------------------------------

1. 변경 배경

기존 회로 엔진은 Battery_Pos / Battery_Neg를 기준으로
회로 연결 여부와 LED 점등 여부를 판정하도록 설계되어 있었다.

또한 전압 helper도 Battery 객체를 기준으로 값을 계산했다.

하지만 현재 프로젝트는 배터리 오브젝트를 사용하지 않고,
브레드보드 자체가 전원에 연결되어 있는 설정을 사용한다.

따라서 아래 두 가지를 함께 바꿔야 한다.

1. 상태 판정 기준
- Battery 기준 제거
- Power rail 기준 사용

2. helper 기준
- Battery 기반 helper 제거 또는 rail 기반 helper로 교체

--------------------------------

2. 현재 상태

현재 evaluator는 이미 상태 판정 기준을
Battery에서 rail 기준으로 일부 변경한 상태다.

현재 상태 판정 기준:
- TopPlus_* / BottomPlus_* 에서
- TopMinus_* / BottomMinus_* 로 가는 경로 존재 여부

LED 판정 기준:
- plus rail -> LED anode
- LED cathode -> minus rail

즉 Open / LedOn / Series / Parallel / Short 판정은
이미 rail 기준으로 동작 중이다. :contentReference[oaicite:0]{index=0}

하지만 아래 helper는 아직 Battery 기반이다.

- GetPrimaryBatteryVoltage()
- CountActiveLoads()
- CalculateSeriesVoltagePerLoad()
- CalculateParallelBranchVoltage()
- GetPrimaryBattery()

즉 상태 판정과 helper 기준이 서로 다르게 남아 있는 상태다. :contentReference[oaicite:1]{index=1}

--------------------------------

3. 이번 단계 핵심 목표

이번 단계 목표는
전원 기준을 완전히 rail 기반으로 통일하는 것이다.

즉:

- Battery 관련 lookup 제거
- Battery helper 제거 또는 rail helper로 대체
- 이후 계산/설명/UI/이펙트가 모두 같은 기준을 보게 만들기

--------------------------------

4. 변경 원칙

1. 기존 그래프 구조는 유지한다
2. Node = socket.nodeId 규칙 유지
3. Edge = wire / active switch 규칙 유지
4. Evaluate는 orchestrator 구조 유지
5. service layer / breadboard bridge는 수정하지 않는다
6. CircuitState enum은 건드리지 않는다
7. BreadboardNodeMapper는 건드리지 않는다

--------------------------------

5. 새 전원 기준

전원 + 후보:
- TopPlus_0 ~ TopPlus_4
- BottomPlus_0 ~ BottomPlus_4

전원 - 후보:
- TopMinus_0 ~ TopMinus_4
- BottomMinus_0 ~ BottomMinus_4

초기 해석 규칙:
- plus rail 집합 중 하나에서
- minus rail 집합 중 하나로
유효 경로가 존재하면 회로 연결로 본다

LED 정상 조건:
- any plus rail -> led anode
- led cathode -> any minus rail

--------------------------------

6. helper 변경 방향

기존 battery helper를 아래 방향으로 변경한다.

기존 제거/축소 대상:
- GetPrimaryBatteryVoltage()
- GetPrimaryBattery()

변경 또는 대체 대상:
- GetRailSupplyVoltage()
- CalculateSeriesVoltagePerLoad()
- CalculateParallelBranchVoltage()

--------------------------------

7. rail 기반 helper 설계

권장 helper 예시:

1. GetRailSupplyVoltage()
역할:
- 현재 프로젝트 기본 공급 전압 반환
- 초기값은 고정 상수 사용 가능
예:
- 3.0V
- 5.0V

이유:
- 현재는 실제 Battery 객체가 없음
- 브레드보드가 외부 전원에 연결된 설정이므로
  공급 전압을 시스템 설정값으로 보는 편이 자연스럽다

2. CountActiveLoads()
역할:
- 현재 회로에서 전압 분배 대상으로 볼 부하 수 계산
- 1차 기준:
  - 연결된 LED
  - 이후 필요하면 Resistor 포함 확장

3. CalculateSeriesVoltagePerLoad()
역할:
- rail 공급 전압 / 부하 수

4. CalculateParallelBranchVoltage()
역할:
- rail 공급 전압 그대로 반환

--------------------------------

8. 공급 전압 해석

현재 프로젝트는 배터리가 없으므로
공급 전압은 아래 둘 중 하나로 처리한다.

방향 A
- evaluator 내부 상수
예:
- const float DefaultRailVoltage = 3.0f;

방향 B
- 별도 설정값 주입
예:
- CircuitRuntimeRoot 또는 settings object에서 공급 전압 제공

현재 단계 권장:
- 우선 상수값으로 단순 유지
- 나중에 settings 주입 구조로 확장 가능

--------------------------------

9. CountActiveLoads 기준

현재 단계에서는 교육용 단순화를 유지한다.

기본 규칙:
- LED = 부하 1개
- Resistor도 나중에 부하 개수에 포함 가능

이번 단계에서는 최소 수정 원칙에 따라:
- 기존 LED 기준 CountActiveLoads 유지 가능
- 필요 시 Resistor 포함은 다음 단계에서 확장

--------------------------------

10. 이번 단계 완료 기준

1. evaluator 내부에 Battery lookup이 더 이상 상태/도우미 계산 기준으로 남아 있지 않다
2. 전원 기준이 전부 rail 기준으로 통일된다
3. CalculateSeriesVoltagePerLoad가 battery 없이도 값 반환 가능하다
4. CalculateParallelBranchVoltage가 battery 없이도 값 반환 가능하다
5. Breadboard bridge / service / mapper는 수정하지 않는다

--------------------------------

11. 핵심 한 줄 요약

이번 수정의 핵심은
"회로 엔진의 전원 기준을 Battery에서 Breadboard power rail로 완전히 통일하는 것"이다.