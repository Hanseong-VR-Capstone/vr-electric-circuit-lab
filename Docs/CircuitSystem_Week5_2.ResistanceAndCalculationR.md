문서명: Week5_ResistanceAndCalculationResultDesign_v1

목적
전류 이펙트 연동은 보류하고,
다음 계산 단계로 넘어가기 위해 저항 데이터 구조와 계산 결과 구조를 먼저 정리한다.

--------------------------------

1. 현재 상태

현재 회로 엔진은 다음을 이미 지원한다.

- 브레드보드 hole -> socket 연결
- rail 기반 전원 구조
- Open / LedOn / Series / Parallel / Short 판정
- 전류 흐름 wire 판정 helper

하지만 아직 회로 계산 단계에서는
저항을 회로 엔진 내부 데이터로 명확히 다루지 않는다.

따라서 다음 단계에서는
"저항을 계산 가능한 부품으로 표현하는 구조"를 먼저 확정해야 한다.

--------------------------------

2. 이번 단계 핵심 목표

이번 단계 목표는 두 가지다.

1. 저항 데이터 구조 정의
2. 계산 결과 구조 정의

즉 아직 실제 계산 helper 전체를 구현하는 단계가 아니라,
그 계산이 들어갈 그릇을 먼저 정하는 단계다.

--------------------------------

3. 저항 데이터 구조 필요성

현재 프로젝트에는 저항 파트가 존재한다.

하지만 현재 회로 엔진 핵심 데이터에는
CircuitResistor 같은 전용 구조가 없다.

저항을 단순 wire처럼만 취급하면
회로 연결 판정은 가능해도
다음 계산은 불가능하다.

- 전체 저항 계산
- 직렬 전압 분배
- 병렬 전류 분배
- 옴의 법칙 계산

따라서 저항 전용 데이터 구조가 필요하다.

--------------------------------

4. 저항 데이터 구조 방향

권장 클래스:
- CircuitResistor

권장 필드:
- resistorId
- pinAId
- pinBId
- resistanceOhms

설명

resistorId
- 저항 부품의 고유 식별자

pinAId / pinBId
- 저항 양 끝 핀의 pinId

resistanceOhms
- 저항값
- 예: 220f, 10000f

의미
- 저항은 회로 판정용 edge가 아니라
  계산용 부품 데이터로 사용한다
- 연결 자체는 여전히 pin -> socket -> node 기준 유지
- 실제 전류/전압 계산 시 저항값을 참조한다

--------------------------------

5. CircuitContext 확장 방향

CircuitContext에 아래 컬렉션 추가를 고려한다.

- resistors

그리고 lookup helper를 추가할 수 있다.

예:
- GetResistorById(string resistorId)

규칙
- 기존 구조를 깨지 않는다
- resistor는 별도 목록으로 관리한다
- wire / switch / led / resistor 책임을 섞지 않는다

--------------------------------

6. 계산 결과 구조 필요성

앞으로 계산 helper가 반환해야 하는 값이 많아진다.

예:
- 전체 전압
- 전체 전류
- 전체 저항
- 부하별 전압
- branch별 전류

이 값을 따로따로 반환하면 구조가 지저분해진다.

따라서 계산 전용 결과 객체가 필요하다.

--------------------------------

7. 계산 결과 구조 방향

권장 클래스:
- CircuitCalculationResult

권장 필드 예시:
- hasValidCircuit
- totalVoltage
- totalCurrent
- totalResistance
- perLoadVoltage
- perBranchCurrent
- activeLoadCount

설명

hasValidCircuit
- 계산 가능한 회로인지 여부

totalVoltage
- 현재 공급 전압

totalCurrent
- 전체 회로 전류

totalResistance
- 전체 저항

perLoadVoltage
- 부하별 전압 정보
- 단순 1차 버전에서는 Dictionary<string, float> 가능

perBranchCurrent
- branch별 전류 정보
- 단순 1차 버전에서는 Dictionary<string, float> 가능

activeLoadCount
- 현재 계산 대상 부하 수

--------------------------------

8. 이번 단계에서의 단순화 원칙

이번 단계에서는 아래를 유지한다.

1. 교육용 단순화 유지
2. 정밀 회로 시뮬레이터를 만들지 않음
3. 계산 helper와 Evaluate를 완전히 분리
4. 데이터 구조 먼저 확정
5. 저항은 양 끝 핀과 저항값만 우선 저장

--------------------------------

9. 아직 하지 않을 것

이번 단계에서는 아래를 하지 않는다.

- 실제 전체 저항 계산 구현
- 옴의 법칙 helper 전체 구현
- 병렬 branch 정밀 해석
- resistor를 current flow analyzer에 포함
- UI 연결

즉 이번 단계는
"계산을 위한 데이터 구조 준비"
단계다.

--------------------------------

10. 완료 기준

1. CircuitResistor 구조가 정의된다
2. CircuitContext에 resistor 목록을 둘 방향이 정해진다
3. CircuitCalculationResult 구조가 정의된다
4. 이후 옴의 법칙 helper를 붙일 수 있는 준비가 된다

--------------------------------

11. 핵심 한 줄 요약

이번 단계의 핵심은
"저항을 계산 가능한 회로 부품으로 정의하고,
계산 결과를 담을 구조를 먼저 확정하는 것"이다.