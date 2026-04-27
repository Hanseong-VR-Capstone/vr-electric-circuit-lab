# CircuitSystem_Week6_3.WireEffectIntegration_v1

## 목적
CircuitCurrentFlowAnalyzer에서 나온 WireDirections를 JumperWireLight에 연결한다.

## 현재 완료 상태
- 회로 판정 정상
- Series / Parallel 계산 정상
- CurrentFlowWireDirection 생성 정상
- 방향 데이터:
  - wireId
  - fromPinId
  - toPinId
  - fromNodeId
  - toNodeId

## 이펙트 담당 스크립트
JumperWireLight 사용:
- LightOn(PinRole.Wire_A)
- LightOn(PinRole.Wire_B)
- LightOff()

## 연결 방식
새 스크립트 CircuitWireEffectController를 만든다.

역할:
1. CircuitCurrentFlowAnalyzer.Analyze() 실행
2. flowResult.WireDirections 순회
3. wireId에 맞는 JumperWireLight 찾기
4. fromPinId가 Wire_A면 LightOn(Wire_A)
5. fromPinId가 Wire_B면 LightOn(Wire_B)
6. 흐르지 않는 wire는 LightOff()

## 주의
- JumperWireLight 자체는 수정하지 않는다.
- 계산 로직 수정 없음.
- Evaluator 수정 없음.
- WireDirections를 기준으로만 이펙트 제어.