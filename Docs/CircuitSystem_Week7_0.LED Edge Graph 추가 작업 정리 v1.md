# LED Edge Graph 추가 작업 정리 v1

## 목적

현재 LED는 CircuitContext에 CircuitLed로 등록되고, CircuitState.LedOn일 때 LedPart.manageLight(true)를 통해 기본 이펙트 연동까지 가능하다.

하지만 현재 LED는 graph edge로 참여하지 않는다.

그래서 실제 회로에서는 LED의 anode와 cathode가 하나의 부품 내부에서 연결되어 있어야 하지만, 현재 시스템에서는 LED anode와 cathode 사이가 graph에서 끊긴 상태로 판정된다.

예를 들어 아래 구조는 실제로는 정상 회로여야 한다.

```text
Plus Rail
-> Wire
-> LED Anode
   LED Cathode
-> Wire
-> Minus Rail
```

하지만 현재 evaluator graph에서 LED anode와 cathode 사이 edge가 없기 때문에:

```text
Plus Rail -> LED Anode
```

에서 경로가 끊긴 것으로 판단된다.

결과적으로 LED 양쪽을 추가 점퍼로 이어야 LedOn이 되는 비정상 구조가 발생한다.

이번 작업의 목적은 LED를 실제 회로 graph의 edge로 추가하여, LED 자체가 anode node와 cathode node를 연결하는 부품으로 동작하게 만드는 것이다.

---

## 현재 문제

현재 graph edge 대상:

```text
Wire
Switch
Resistor
```

현재 누락된 edge 대상:

```text
LED
```

현재 문제 상황:

```text
Plus Rail
-> Wire
-> LED Anode
   LED Cathode
-> Wire
-> Minus Rail
```

기대 동작:

```text
CircuitState.LedOn
```

현재 동작:

```text
Open
```

또는 LED anode와 cathode 사이를 점퍼와이어로 추가 연결해야 LedOn이 되는 이상한 상태가 발생한다.

---

## 수정 목표

이번 작업의 수정 목표는 다음과 같다.

1. CircuitEvaluator의 graph 생성 과정에 LED edge를 추가한다.
2. CircuitCurrentFlowAnalyzer의 path 탐색에도 LED edge를 포함한다.
3. LED edge는 anode node와 cathode node를 연결한다.
4. LED는 wire가 아니므로 ActiveWireIds에는 포함하지 않는다.
5. LED는 resistor처럼 path 통과용 edge 역할을 한다.
6. LedOn 판정은 기존처럼 plus rail -> anode -> cathode -> minus rail 기준을 유지한다.
7. LED 개별 ON/OFF 판정은 이번 작업에서 하지 않는다.

---

## 핵심 설계

LED는 두 가지 의미를 가진다.

### 1. 회로 판정 관점

LED는 anode와 cathode 사이를 연결하는 edge이다.

즉:

```text
LED anode node <-> LED cathode node
```

를 graph에 추가해야 한다.

### 2. 극성 판정 관점

LED는 방향성이 있는 부품이다.

정상 점등 조건은:

```text
Plus Rail -> LED Anode -> LED Cathode -> Minus Rail
```

이다.

따라서 LED edge를 graph에 추가하더라도 LedOn 판정에서는 정방향 조건을 유지해야 한다.

---

## 방향성 처리 기준

이번 작업에서는 graph 자체는 기존 구조와 맞추기 위해 양방향 edge로 추가한다.

즉:

```text
anode <-> cathode
```

형태로 graph에 추가한다.

이유:

1. 현재 graph 구조는 Wire / Switch / Resistor 모두 양방향 edge 기반이다.
2. graph 전체를 방향 그래프로 바꾸면 기존 evaluator, current flow, series/parallel 계산에 영향이 매우 크다.
3. 현재 목적은 LED를 graph에 참여시키는 것이다.
4. LED 방향성 자체는 기존 LedOn 판정 helper에서 유지하는 편이 안전하다.

즉:

```text
graph edge:
anode <-> cathode

LedOn condition:
plus rail -> anode
cathode -> minus rail
```

구조로 유지한다.

---

## 수정 대상

수정 대상 파일:

```text
CircuitEvaluator.cs
CircuitCurrentFlowAnalyzer.cs
```

수정 금지 파일:

```text
CircuitContext.cs
CircuitConnectionService.cs
CircuitLedRegistrar.cs
CircuitLedEffectController.cs
CircuitCalculationHelper.cs
BreadboardHoleBridge.cs
BreadboardNodeMapper.cs
BreadboardSocketBootstrap.cs
```

---

## CircuitEvaluator 수정 방향

BuildNodeGraph() 내부에 AddLedEdges() helper를 추가한다.

예상 구조:

```csharp
private Dictionary<string, List<string>> BuildNodeGraph()
{
    var graph = new Dictionary<string, List<string>>();

    AddWireEdges(graph);
    AddSwitchEdges(graph);
    AddResistorEdges(graph);
    AddLedEdges(graph);

    return graph;
}
```

---

## AddLedEdges 규칙

1. context.Leds를 순회한다.
2. led.anodePinId로 anodeNode를 찾는다.
3. led.cathodePinId로 cathodeNode를 찾는다.
4. 둘 다 유효하면 graph에 edge를 추가한다.
5. anodeNode == cathodeNode이면 무시한다.
6. pin 또는 socket 연결이 없으면 edge를 추가하지 않는다.

추가 결과:

```text
Plus Rail
-> Wire
-> LED Anode
-> LED Cathode
-> Wire
-> Minus Rail
```

구조가 실제 graph에서도 연결된 상태가 된다.

---

## CircuitCurrentFlowAnalyzer 수정 방향

CurrentFlowAnalyzer도 rail-to-rail path 탐색 시 LED edge를 포함해야 한다.

이유:

LED가 path에 포함되지 않으면 정상 회로인데도 전류 흐름 분석이 끊긴 것으로 판정될 수 있다.

규칙:

1. path 탐색 graph에 LED edge 포함
2. LED는 ActiveWireIds에 넣지 않음
3. LED는 WireDirections에 넣지 않음
4. LED는 path 통과용 edge로만 사용
5. resistor와 동일한 개념으로 처리

즉:

```text
path 탐색:
LED 포함

wire effect:
LED 제외
```

구조를 유지한다.

---

## 이번 작업에서 하지 않을 것

이번 작업에서는 아래를 하지 않는다.

```text
LED 개별 ON/OFF 판정
LED별 current path ownership 분석
State == LedOn 시 모든 LED 켜지는 문제 해결
고유 ID 시스템 개편
Prefab / Clone 대응
복수 독립 회로 지원
CircuitState enum 변경
graph 전체 방향 그래프화
```

---

## 예상 결과

### 수정 전

```text
Plus Rail
-> Wire
-> LED Anode

LED Cathode
-> Wire
-> Minus Rail
```

결과:

```text
Open
```

또는 LED 양쪽을 점퍼로 이어야 정상 동작.

---

### 수정 후

```text
Plus Rail
-> Wire
-> LED Anode
-> LED Cathode
-> Wire
-> Minus Rail
```

결과:

```text
CircuitState.LedOn
IsFlowing = true
```

또한:

```text
ActiveWireIds
```

에는 실제 점퍼와이어만 포함된다.

---

## 테스트 케이스

### Test 1. 정상 LED 회로

구성:

```text
BottomPlus_2
-> Wire
-> LED Anode
-> LED Cathode
-> Wire
-> BottomMinus_2
```

기대 결과:

```text
CircuitState.LedOn
IsFlowing = true
ActiveWireIds contains both wires
```

---

### Test 2. LED 반대 연결

구성:

```text
Plus Rail
-> Wire
-> LED Cathode
-> LED Anode
-> Wire
-> Minus Rail
```

기대 결과:

```text
Not LedOn
```

최종 상태는 기존 evaluator 정책을 따른다.

---

### Test 3. LED 한쪽만 연결

구성:

```text
Plus Rail
-> Wire
-> LED Anode

LED Cathode disconnected
```

기대 결과:

```text
CircuitState.Open
IsFlowing = false
```

---

### Test 4. LED + 저항 회로

구성:

```text
Plus Rail
-> Wire
-> Resistor
-> LED Anode
-> LED Cathode
-> Wire
-> Minus Rail
```

기대 결과:

```text
CircuitState.LedOn
CurrentFlowAnalyzer 정상 동작
CalculationHelper 정상 동작
```

---

## 완료 기준

1. LED가 graph edge로 추가된다.
2. LED 양쪽을 점퍼로 추가 연결하지 않아도 정상 회로가 닫힌다.
3. LED 방향이 맞으면 LedOn이 된다.
4. LED 방향이 틀리면 LedOn이 되지 않는다.
5. CurrentFlowAnalyzer가 LED를 포함한 path를 찾을 수 있다.
6. ActiveWireIds에는 wire만 포함된다.
7. 기존 resistor 계산 구조가 깨지지 않는다.
8. 기존 wire effect, breadboard effect, UI query 구조가 깨지지 않는다.

---

## 핵심 한 줄 요약

이번 작업의 핵심은 LED를 단순 등록 데이터가 아니라 anode와 cathode를 연결하는 실제 graph edge 부품으로 추가하는 것이다.