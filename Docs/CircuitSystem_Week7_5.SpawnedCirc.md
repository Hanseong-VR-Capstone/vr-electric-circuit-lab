# CircuitSystem_Week7_5.SpawnedCircuitPartInitialization_v1

## 목적

Workbench UI 버튼으로 소환되는 부품 clone이 회로 시스템에 안정적으로 등록되도록 한다.

현재 ObjectSpawner는 prefab을 단순 Instantiate만 한다.

```csharp
Instantiate(selected, entry.spawnPoint.position, entry.spawnPoint.rotation);
```

이후에:
- runtimeRoot 주입
- unique id 부여
- registrar 재등록
- clone용 pinId 정리

같은 후처리가 없다.

그래서 같은 prefab을 여러 번 소환하면 GameObject.name 기반 ID가 충돌하거나,
registrar가 CircuitRuntimeRoot를 찾지 못해 context 등록이 실패할 수 있다.

---

## 현재 문제

현재 registrar들은 대부분 다음 방식으로 runtimeRoot를 찾는다.

```csharp
runtimeRoot = GetComponentInParent<CircuitRuntimeRoot>();
```

하지만 ObjectSpawner는 clone을 world에 바로 생성하고 parent를 지정하지 않는다.

따라서 생성된 clone은 CircuitRuntimeRoot의 자식이 아니며,
registrar가 runtimeRoot를 자동으로 찾지 못할 가능성이 크다.

또한 현재 ID는 대부분 GameObject.name 기반이다.

예:

```text
wireId = name
resistorId = resistorPart.name
ledId = ledPart.name
switchId = switchPart.name
pinId = parentPart.name + "_" + pinRole
```

Prefab clone은 같은 이름을 가질 수 있으므로 ID 충돌 가능성이 있다.

---

## 현재 목표

프리팹 소환 직후 clone에 unique id와 runtimeRoot를 부여하고,
각 registrar가 그 값을 기준으로 회로 context에 등록되도록 만든다.

---

## 핵심 흐름

```text
ObjectSpawner
-> prefab clone Instantiate
-> CircuitSpawnedPartInitializer 실행
-> uniquePartId 생성
-> clone 이름 정리
-> 내부 registrar에 runtimeRoot + idOverride 주입
-> registrar가 CircuitContext에 등록
```

---

## 수정 방향

ObjectSpawner는 생성만 담당하고,
회로 초기화 책임은 새 초기화 스크립트로 분리한다.

새 스크립트 후보:

```text
CircuitSpawnedPartInitializer.cs
```

역할:
- clone용 uniquePartId 생성
- CircuitRuntimeRoot 찾기
- clone 내부 registrar들에게 runtimeRoot 주입
- clone 내부 registrar들에게 idOverride 주입
- registrar 등록 실행 또는 재실행

---

## 수정 대상

수정 가능 파일:
- ObjectSpawner.cs
- CircuitWireRegistrar.cs
- CircuitResistorRegistrar.cs
- CircuitLedRegistrar.cs
- CircuitSwitchRegistrar.cs

추가 가능 파일:
- CircuitSpawnedPartInitializer.cs

수정 금지 파일:
- CircuitContext.cs
- CircuitConnectionService.cs
- CircuitEvaluator.cs
- CircuitCurrentFlowAnalyzer.cs
- CircuitCalculationHelper.cs
- BreadboardHoleBridge.cs
- BreadboardNodeMapper.cs
- BreadboardSocketBootstrap.cs
- effect controller 계열

---

## Registrar 수정 방향

각 registrar는 외부 초기화 API를 제공한다.

예상 형태:

```text
InitializeForSpawnedPart(CircuitRuntimeRoot runtimeRoot, string idOverride)
```

또는 기존 코드 스타일에 맞는 유사한 이름 사용.

각 registrar는:
- runtimeRoot가 주입되면 해당 값을 사용
- idOverride가 있으면 GameObject.name 대신 idOverride 사용
- idOverride가 없으면 기존 방식 유지

즉 기존 씬 배치 부품이 깨지면 안 된다.

---

## ID 규칙

소환된 clone은 uniquePartId를 가진다.

예:

```text
VR Led_001
VR Led_002
VR JumperWire_001
VR 10K Resistor_001
```

pinId도 이 uniquePartId를 기준으로 생성되어야 한다.

예:

```text
VR Led_001_LED_Plus
VR Led_001_LED_Minus
VR JumperWire_001_Wire_A
VR JumperWire_001_Wire_B
```

중요:
BreadboardHoleBridge가 resolve하는 pinId와
registrar가 등록하는 pinId가 반드시 같아야 한다.

---

## 이번 단계에서 하지 않을 것

이번 단계에서는 effect binding 자동 등록은 하지 않는다.

제외:
- CircuitWireEffectController 자동 binding
- CircuitLedEffectController 자동 binding
- Breadboard visual binding 변경

이 작업은 다음 단계에서 별도로 진행한다.

이번 단계 목표는 회로 등록 안정화다.

---

## 완료 기준

1. ObjectSpawner가 clone 생성 후 초기화 스크립트를 호출한다.
2. clone마다 unique id가 부여된다.
3. registrar가 runtimeRoot를 안정적으로 받는다.
4. wire/resistor/LED/switch clone이 CircuitContext에 중복 없이 등록된다.
5. BreadboardHoleBridge의 pinId와 registrar pinId가 일치한다.
6. 기존 씬 배치 부품 동작이 깨지지 않는다.

---

## 핵심 한 줄 요약

Workbench에서 소환된 부품 clone이 unique id와 runtimeRoot를 받아 회로 context에 안정적으로 등록되도록 한다.