# CircuitSystem_Week7_2.ReverseLedCurrentFlowBlock_v1

## 목적

현재 역방향 LED도 CurrentFlowAnalyzer 기준에서는 전류가 흐르는 것으로 판정되는 문제를 수정한다.

현재 구조에서는 LED edge가 graph에 양방향으로 추가되어 있기 때문에:

```text
Plus -> Cathode -> Anode -> Minus
```

같은 역방향 연결도 rail-to-rail path로 인식된다.

그 결과:
- LedOn은 정상적으로 차단됨
- 하지만 IsFlowing = true
- wire current effect 발생
- breadboard current visual 발생

상태가 된다.

---

## 현재 문제

현재 구조:

```text
graph connectivity
```

와

```text
LED polarity validation
```

이 분리되어 있다.

그래서:
- evaluator는 역방향 LED를 LedOn으로 막음
- 하지만 CurrentFlowAnalyzer는 단순 rail-to-rail path만 보기 때문에 역방향 LED도 flowing 처리한다.

결과:
- 역방향 LED인데 전류 이펙트 발생

---

## 현재 목표

역방향 LED path는 CurrentFlowAnalyzer에서도 전류 흐름으로 인정하지 않도록 수정한다.

즉:
- 정상 방향 LED만 flowing 가능
- 역방향 LED는 flowing 불가

---

## 핵심 수정 방향

CurrentFlowAnalyzer에서도 polarity validation 개념을 추가한다.

현재:
```text
plus rail -> minus rail path 존재
= IsFlowing true
```

수정 후:
```text
plus -> anode
cathode -> minus
```

정상 polarity인 경우만 flowing 허용.

역방향:
```text
plus -> cathode
anode -> minus
```

이면 해당 path는 invalid 처리한다.

---

## 구조 유지 규칙

1. graph는 양방향 유지
2. LED edge 구조 유지
3. evaluator 구조 유지
4. calculation 구조 유지
5. UI 구조 유지
6. effect controller 구조 유지
7. graph 전체 방향화 금지

---

## 수정 대상

수정 가능 파일:
- CircuitCurrentFlowAnalyzer.cs

수정 금지 파일:
- CircuitEvaluator.cs
- CircuitCalculationHelper.cs
- CircuitConnectionService.cs
- CircuitLedRegistrar.cs
- CircuitWireEffectController.cs
- BreadboardCurrentVisualController.cs

---

## 수정 방향

CurrentFlowAnalyzer 내부에:

```text
polarity validation graph
```

를 추가한다.

구조:
- wire
- switch
- resistor

만 포함.

LED edge는 polarity graph에 포함하지 않는다.

이 graph 기준으로:
- plus -> anode
- cathode -> minus

만 허용한다.

역방향:
- plus -> cathode
- anode -> minus

이면 flowing 실패 처리.

---

## 기대 결과

정상 방향:

```text
Plus -> Anode -> Cathode -> Minus
```

결과:
```text
IsFlowing = true
wire effect ON
breadboard effect ON
```

역방향:

```text
Plus -> Cathode -> Anode -> Minus
```

결과:
```text
IsFlowing = false
wire effect OFF
breadboard effect OFF
```

LedOn은 기존처럼 evaluator가 처리한다.

---

## 완료 기준

1. 역방향 LED는 IsFlowing false
2. 역방향 LED는 wire effect 발생 안 함
3. 정상 방향 LED는 기존처럼 flowing 가능
4. graph 구조 유지
5. evaluator 구조 영향 없음
6. calculation 구조 영향 없음

---

## 핵심 한 줄 요약

역방향 LED path는 CurrentFlowAnalyzer에서도 invalid 처리하여 전류 이펙트가 발생하지 않도록 수정한다.