# CircuitSystem_Week7_3.FourPinSwitchModel_v1

## 목적

현재 임시 2핀 switch 구조를 실제 4핀 breadboard tact switch 구조로 변경한다.

현재 switch는:

```text
pinA <-> pinB
```

만 사용하는 단순 구조다.

하지만 실제 4핀 tact switch는:

```text
(0,1) 그룹
(2,3) 그룹
```

두 개의 내부 그룹을 가진다.

---

## 현재 문제

현재 CircuitSwitch 구조:

```text
switchId
pinAId
pinBId
isOn
```

현재 evaluator는:

```text
ON:
pinA <-> pinB 연결

OFF:
연결 없음
```

만 처리한다.

즉 현재는 사실상 2핀 switch처럼 동작한다.

---

## 목표 구조

실제 4핀 switch 구조를 graph에 반영한다.

기본 내부 연결:

```text
pin0 <-> pin1
pin2 <-> pin3
```

항상 연결 상태 유지.

Switch ON 시 추가 연결:

```text
pin0 <-> pin2
pin0 <-> pin3
pin1 <-> pin2
pin1 <-> pin3
```

즉:

OFF:
```text
(0,1) 그룹
(2,3) 그룹
분리
```

ON:
```text
(0,1) 그룹
(2,3) 그룹
서로 연결
```

---

## 수정 목표

1. CircuitSwitch를 4핀 구조로 확장
2. CircuitSwitchRegistrar에서 4개 pin 등록
3. CircuitEvaluator의 switch edge 생성 수정
4. CircuitCurrentFlowAnalyzer의 switch edge 생성 수정
5. 기존 SwitchStateChanged 구조 유지

---

## 핵심 구조

현재:

```text
switch edge 1개
```

수정 후:

```text
always-connected edge
+
ON-only edge
```

구조로 분리.

---

## CircuitSwitch 구조 변경

현재:

```text
switchId
pinAId
pinBId
isOn
```

수정 후:

```text
switchId
pin0Id
pin1Id
pin2Id
pin3Id
isOn
```

---

## helper 구조

추가 helper:

```text
GetAlwaysConnectedPairs()
GetOnConnectedPairs()
GetActiveConnectedPairs()
```

역할:

always:
```text
pin0 <-> pin1
pin2 <-> pin3
```

on:
```text
pin0 <-> pin2
pin0 <-> pin3
pin1 <-> pin2
pin1 <-> pin3
```

active:
- always
- switch on이면 on pairs 추가

---

## CircuitEvaluator 수정 방향

현재:

```text
pinA <-> pinB
```

1개 edge 추가.

수정 후:

```text
switch.GetActiveConnectedPairs()
```

순회 후 모든 edge 추가.

규칙:
- disconnected pin skip
- invalid node skip
- same-node skip
- bidirectional edge 유지

---

## CircuitCurrentFlowAnalyzer 수정 방향

Evaluator와 동일한 switch edge 구조 사용.

즉:
- switch edge traversal 가능
- activeWireIds에는 switch 미포함
- WireDirections에도 switch 미포함

---

## 유지 규칙

1. Node = socket.nodeId 유지
2. graph 양방향 유지
3. switch를 wire로 바꾸지 않음
4. calculation 구조 유지
5. UI/effect 구조 유지
6. SwitchStateChanged 유지
7. service layer 최소 수정 유지

---

## 기대 결과

OFF:

```text
(0,1) 그룹
(2,3) 그룹
분리
```

결과:
- 회로 open
- 전류 이펙트 없음
- LED off

ON:

```text
(0,1) 그룹
(2,3) 그룹
연결
```

결과:
- 회로 close 가능
- 전류 흐름 가능
- LED 점등 가능

---

## 완료 기준

1. 4핀 switch 정상 등록
2. OFF 상태에서 그룹 분리
3. ON 상태에서 그룹 연결
4. evaluator 정상 동작
5. current flow 정상 동작
6. 기존 calculation 구조 영향 없음
7. 기존 UI/effect 구조 영향 없음

---

## 핵심 한 줄 요약

현재 2핀 switch 구조를 실제 4핀 tact switch의 내부 연결 구조로 확장한다.