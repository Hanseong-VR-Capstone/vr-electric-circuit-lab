# CircuitSystem_Week6_7.LedPolarityValidationFix_v1

## 목적

LED edge graph 추가 이후,
반대 방향으로 연결된 LED도 LedOn으로 판정되는 문제를 수정한다.

현재 LED는 graph edge로 정상 추가되어:

```text
LED anode node <-> LED cathode node
```

연결이 가능해졌다.

하지만 현재 polarity validation helper가
실제 방향성 자체를 충분히 검증하지 않기 때문에:

```text
Plus Rail
-> LED Cathode
-> LED Anode
-> Minus Rail
```

처럼 완전히 반대로 연결된 경우에도 LedOn이 발생한다.

---

## 현재 문제

현재 graph는 양방향 edge 구조다.

즉 LED edge도:

```text
anode <-> cathode
```

로 추가되어 있다.

이 구조 자체는 의도된 설계다.

문제는 현재 LedOn validation helper가:

```text
plus -> anode path 존재
cathode -> minus path 존재
```

정도만 검사하고 있다는 점이다.

그래서 실제로는:

```text
plus -> cathode -> anode
```

같은 역방향 path도 HasPath() 기준으로 true가 되어버린다.

결과:
- LED 극성이 반대인데도 LedOn 발생

---

## 현재 목표

현재 구조를 유지한 상태에서
LedOn polarity validation만 강화한다.

즉:
- graph는 양방향 유지
- evaluator 구조 유지
- current flow 구조 유지

대신:
- polarity helper에서 역방향 연결을 명시적으로 차단

---

## 핵심 수정 방향

현재 정상 조건:

```text
plus -> anode
cathode -> minus
```

추가해야 할 실패 조건:

```text
plus -> cathode
anode -> minus
```

즉:

정상 방향만 허용하고,
역방향 path가 존재하면 LedOn 실패 처리한다.

---

## 수정 대상

수정 가능 파일:
- CircuitEvaluator.cs

필요 시 최소 수정 가능:
- polarity helper 내부 private helper

수정 금지 파일:
- CircuitCurrentFlowAnalyzer.cs
- CircuitConnectionService.cs
- CircuitCalculationHelper.cs
- CircuitLedRegistrar.cs
- BreadboardHoleBridge.cs
- BreadboardNodeMapper.cs
- BreadboardSocketBootstrap.cs

---

## 수정 방향

현재 polarity helper 내부에 아래 개념 추가:

정상 조건:
```text
plus -> anode
cathode -> minus
```

역방향 실패 조건:
```text
plus -> cathode
anode -> minus
```

즉:

```text
if (plus reaches cathode)
{
    invalid polarity
}

if (anode reaches minus)
{
    invalid polarity
}
```

를 추가한다.

---

## 중요한 구조 유지 규칙

1. graph 자체는 양방향 유지
2. LED edge 구조 유지
3. HasPath() helper 유지
4. CurrentFlowAnalyzer 수정 금지
5. graph 전체를 방향 그래프로 바꾸지 않음
6. evaluator 구조 유지
7. helper 기반 polarity validation 유지

---

## 현재 구조 의도

현재 프로젝트 구조는:

```text
graph connectivity
```

와

```text
LED polarity validation
```

를 분리한다.

즉:
- graph는 연결 여부만 담당
- polarity helper가 방향성 담당

이번 수정은 그 구조를 유지하면서
polarity validation만 강화하는 작업이다.

---

## 기대 결과

정상 연결:

```text
Plus -> Anode -> Cathode -> Minus
```

결과:
```text
LedOn
```

반대 연결:

```text
Plus -> Cathode -> Anode -> Minus
```

결과:
```text
Not LedOn
```

기존 evaluator 정책에 따라:
- Open
- Short
- 기타 비정상 상태

중 하나 반환.

---

## 완료 기준

1. 정상 방향 LED만 LedOn 가능
2. 반대 방향 LED는 LedOn 불가
3. graph 구조는 그대로 유지
4. LED edge 구조 유지
5. current flow 구조 영향 없음
6. resistor 계산 구조 영향 없음
7. 기존 evaluator 흐름 유지

---

## 핵심 한 줄 요약

이번 수정의 핵심은 graph는 양방향 유지한 채,
LedOn polarity validation에서 역방향 연결을 명시적으로 차단하는 것이다.