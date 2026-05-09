# CircuitSystem_Week7_7.PerLedEffectControl_v1

## 목적

현재 LED effect가 CircuitState.LedOn 하나만 보고 동작해서,
회로에 포함되지 않은 LED까지 함께 켜지는 문제를 수정한다.

현재 구조:

```text
CircuitState == LedOn
-> 모든 LedPart effect ON
```

문제:
- 실제 current path에 포함된 LED만 켜져야 함
- 현재는 등록된 모든 LED가 동시에 켜질 수 있음

---

## 현재 문제

CircuitEvaluator는 회로 전체 상태만 반환한다.

```text
Open
Series
Parallel
LedOn
Short
```

하지만 LED effect에는 LED별 판정이 필요하다.

즉 필요한 정보는:

```text
이 ledId가 실제 정상 방향 current path에 포함되어 있는가?
```

이다.

---

## 현재 목표

LED effect controller가 전체 CircuitState만 보지 않고,
각 LED별로 실제 점등 가능 여부를 판정하도록 수정한다.

---

## 핵심 방향

새 helper 또는 query 구조를 추가한다.

추천 방향:

```text
CircuitEvaluator 또는 별도 helper
-> IsLedActive(string ledId)
```

역할:
- 해당 LED의 anode/cathode node 확인
- polarity validation graph 사용
- plus -> anode
- cathode -> minus
- reversed polarity는 false
- 조건 만족 시 해당 LED만 ON

---

## 수정 대상

수정 가능 파일:
- CircuitEvaluator.cs
- CircuitLedEffectController.cs

필요 시 최소 수정 가능:
- CircuitQueryService.cs

수정 금지 파일:
- CircuitCurrentFlowAnalyzer.cs
- CircuitCalculationHelper.cs
- CircuitConnectionService.cs
- BreadboardHoleBridge.cs
- ObjectSpawner.cs
- registrar 계열

---

## CircuitEvaluator 수정 방향

LED별 판정 API를 추가한다.

예상 형태:

```text
public bool IsLedActive(string ledId)
```

또는 기존 스타일에 맞는 이름 사용.

판정 기준:

```text
plus -> anode
cathode -> minus
```

그리고 역방향:

```text
plus -> cathode
anode -> minus
```

이면 false.

주의:
- polarity 검사는 LED edge 없는 graph 사용
- full graph의 LED edge로 역방향이 true 되는 문제 방지
- 기존 Evaluate 흐름은 깨지 않음

---

## CircuitLedEffectController 수정 방향

기존:

```text
CircuitState == LedOn
-> 모든 LED ON
```

수정 후:

```text
각 ledBinding 순회
-> evaluator.IsLedActive(ledId)
-> 해당 LED만 manageLight(true)
```

---

## 유지 규칙

1. 전체 CircuitState 구조 변경 금지
2. LED edge 구조 유지
3. graph 전체 방향화 금지
4. current flow 구조 수정 금지
5. calculation 구조 수정 금지
6. 기존 수동/자동 LED binding 유지
7. clone unique id 구조 유지

---

## 기대 결과

LED 2개가 있을 때:

```text
LED_A만 정상 회로 path 포함
LED_B는 연결 안 됨
```

결과:

```text
LED_A ON
LED_B OFF
```

역방향 LED:

```text
Plus -> Cathode
Anode -> Minus
```

결과:

```text
해당 LED OFF
```

---

## 완료 기준

1. 회로에 포함된 LED만 켜진다.
2. 연결되지 않은 LED는 켜지지 않는다.
3. 역방향 LED는 켜지지 않는다.
4. 여러 LED가 있어도 개별적으로 ON/OFF 된다.
5. 기존 evaluator/current flow/calculation 구조가 깨지지 않는다.

---

## 핵심 한 줄 요약

전체 LedOn 상태가 아니라 LED별 path/polarity 판정을 기준으로 개별 LED effect를 제어한다.