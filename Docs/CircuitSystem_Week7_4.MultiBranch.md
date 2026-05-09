# CircuitSystem_Week7_4.MultiBranchCurrentFlow_v1

## 목적

현재 CurrentFlowAnalyzer가 첫 번째 rail-to-rail path만 반환하는 구조를,
모든 active branch를 탐색하는 구조로 확장한다.

현재는 병렬 회로가 존재해도:

```text
첫 번째 발견된 path
```

만 flowing 처리된다.

그 결과:
- 일부 wire만 current effect 활성화
- 일부 breadboard node만 effect 활성화
- 병렬 회로 전체 흐름 표현 불가

상태가 발생한다.

---

## 현재 문제

현재 CurrentFlowAnalyzer 구조:

```text
TryFindRailPath(...)
```

기반.

즉:
- rail-to-rail path 하나만 찾음
- 첫 번째 path 찾으면 종료
- 해당 path 기준으로:
  - ActiveWireIds
  - WireDirections
  생성

현재 구조에서는 병렬 회로가 있어도:

```text
branch 일부만 active
```

될 수 있다.

---

## 현재 목표

CurrentFlowAnalyzer를:

```text
single path
```

기반에서:

```text
all active branches
```

기반으로 확장한다.

즉:
- 모든 rail-to-rail path 탐색
- 모든 active wire 수집
- 모든 active node 수집 가능 구조 준비

---

## 핵심 수정 방향

현재:

```text
TryFindRailPath(...)
```

↓

수정 후:

```text
FindAllRailPaths(...)
```

구조로 변경.

현재:
- path 하나 반환

수정 후:
- path 여러 개 반환

예상 반환:

```text
List<List<string>>
```

형태.

---

## CurrentFlowAnalyzer 변경 방향

현재:
```text
첫 번째 path 기준
```

으로:
- active wire 수집
- WireDirections 생성

수정 후:
```text
모든 path 순회
```

기준으로:
- active wire 누적
- direction 생성
- 중복 제거

---

## 중복 처리 규칙

병렬 회로에서는 동일 node / wire가 여러 path에 포함될 수 있다.

따라서:
- HashSet 기반 중복 제거 유지
- ActiveWireIds 중복 금지
- WireDirections 중복 최소화

---

## 유지 규칙

1. Node = socket.nodeId 유지
2. graph 양방향 유지
3. evaluator 구조 유지
4. calculation 구조 유지
5. UI 구조 유지
6. LED polarity 구조 유지
7. switch edge 구조 유지
8. effect controller 수정 최소화

---

## 수정 대상

수정 가능 파일:
- CircuitCurrentFlowAnalyzer.cs

필요 시 최소 수정 가능:
- CurrentFlow helper 내부 private helper

수정 금지 파일:
- CircuitEvaluator.cs
- CircuitCalculationHelper.cs
- CircuitConnectionService.cs
- CircuitWireEffectController.cs
- BreadboardCurrentVisualController.cs

---

## 기대 결과

현재:
```text
병렬 회로 일부 branch만 flowing
```

수정 후:
```text
병렬 회로 모든 branch flowing
```

즉:
- 모든 active wire effect 활성화
- 모든 active breadboard visual 활성화
- 병렬 회로 흐름 표현 가능

---

## 완료 기준

1. 병렬 회로 모든 branch 탐색 가능
2. ActiveWireIds에 모든 active wire 포함
3. WireDirections가 병렬 branch 기준으로 생성
4. 기존 series 회로 정상 유지
5. evaluator 구조 영향 없음
6. LED polarity 구조 영향 없음

---

## 핵심 한 줄 요약

CurrentFlowAnalyzer를 단일 rail path 기반에서 전체 active branch 탐색 기반으로 확장한다.