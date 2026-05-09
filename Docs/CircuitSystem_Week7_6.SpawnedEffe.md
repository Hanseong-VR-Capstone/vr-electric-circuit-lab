# CircuitSystem_Week7_6.SpawnedEffectBinding_v1

## 목적

Workbench에서 소환된 부품 clone의 이펙트가 자동으로 회로 이펙트 컨트롤러에 등록되도록 한다.

현재 clone 부품은:
- unique id 부여 완료
- runtimeRoot 자동 탐색 완료
- registrar 등록 완료
- CircuitContext 등록 정상

하지만 effect binding은 아직 Inspector 수동 등록 기반이다.

따라서 새로 소환된:
- JumperWire clone의 JumperWireLight
- LED clone의 LedPart

가 기존 effect controller에 자동 등록되지 않는다.

---

## 현재 문제

현재 Wire effect 구조:

```text
CircuitWireEffectController
-> wireBindings 수동 등록
-> wireId와 JumperWireLight 연결
```

현재 LED effect 구조:

```text
CircuitLedEffectController
-> ledBindings 수동 등록
-> ledId와 LedPart 연결
```

문제:
- scene에 미리 있는 부품은 binding 가능
- runtime에 소환된 clone은 binding 목록에 없음
- 따라서 회로 등록은 됐지만 이펙트는 안 켜질 수 있음

---

## 현재 목표

소환된 clone이 초기화될 때,
해당 clone 내부의 effect target을 자동으로 effect controller에 등록한다.

---

## 핵심 흐름

```text
ObjectSpawner
-> prefab clone 생성
-> CircuitSpawnedPartInitializer 초기화
-> uniquePartId 부여
-> registrar 등록
-> effect controller에 binding 자동 추가
```

---

## 수정 방향

CircuitSpawnedPartInitializer에 effect binding 단계를 추가한다.

대상:
- JumperWireLight
- LedPart

연결 대상:
- CircuitWireEffectController
- CircuitLedEffectController

---

## 수정 대상

수정 가능 파일:
- CircuitSpawnedPartInitializer.cs
- CircuitWireEffectController.cs
- CircuitLedEffectController.cs

수정 금지 파일:
- ObjectSpawner.cs
- CircuitEvaluator.cs
- CircuitCurrentFlowAnalyzer.cs
- CircuitCalculationHelper.cs
- CircuitConnectionService.cs
- BreadboardHoleBridge.cs
- BreadboardNodeMapper.cs
- BreadboardSocketBootstrap.cs

---

## CircuitWireEffectController 수정 방향

자동 binding용 public method를 추가한다.

예상 형태:

```text
RegisterWireBinding(string wireId, JumperWireLight light)
```

역할:
- wireId가 비어 있으면 return
- light가 null이면 return
- 같은 wireId가 이미 있으면 중복 추가하지 않음
- 없으면 wireBindings에 추가

기존 수동 binding은 유지한다.

---

## CircuitLedEffectController 수정 방향

자동 binding용 public method를 추가한다.

예상 형태:

```text
RegisterLedBinding(string ledId, LedPart ledPart)
```

역할:
- ledId가 비어 있으면 return
- ledPart가 null이면 return
- 같은 ledId가 이미 있으면 중복 추가하지 않음
- 없으면 ledBindings에 추가

기존 수동 binding은 유지한다.

---

## CircuitSpawnedPartInitializer 수정 방향

기존 초기화 흐름 뒤에 effect binding 등록을 추가한다.

예상 흐름:

```text
InitializeRegistrars(uniquePartId)
RegisterEffectBindings(uniquePartId)
```

RegisterEffectBindings 역할:

1. scene에서 CircuitWireEffectController 찾기
2. clone 내부 JumperWireLight 찾기
3. 있으면 RegisterWireBinding(uniquePartId, jumperWireLight) 호출
4. scene에서 CircuitLedEffectController 찾기
5. clone 내부 LedPart 찾기
6. 있으면 RegisterLedBinding(uniquePartId, ledPart) 호출

---

## 유지 규칙

1. 기존 수동 binding 유지
2. clone 자동 binding은 추가 기능으로만 동작
3. effect controller가 없으면 warning 정도만 출력하고 실패 처리
4. evaluator/current flow/calculation은 수정하지 않음
5. registration id와 effect binding id는 같은 uniquePartId 사용
6. 중복 binding 방지

---

## 완료 기준

1. 소환된 JumperWire clone의 이펙트가 자동 등록된다.
2. 소환된 LED clone의 이펙트가 자동 등록된다.
3. 기존 scene 배치 부품의 수동 binding은 유지된다.
4. 같은 clone이 중복 binding되지 않는다.
5. 회로 등록 id와 effect binding id가 일치한다.
6. evaluator/current flow/calculation 구조 영향 없음

---

## 핵심 한 줄 요약

소환된 wire/LED clone의 시각 이펙트 대상이 effect controller에 자동 binding되도록 한다.