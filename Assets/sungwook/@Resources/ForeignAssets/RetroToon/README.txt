BBANG / RETRO INK — Unity 3D art-style demo
==========================================

Unity 6000.3.22f1 / Universal Render Pipeline 17.3.0
Input System 1.20.0

실행
----
Assets/RetroToon/Scenes/RetroBurgerStreet.unity를 열고 Play를 누르세요.
Tools > BBang > Open Retro Burger Street 메뉴로도 열 수 있습니다.
기본 SampleScene은 보존되어 있습니다. 데모 씬은 Build Settings의 첫 씬입니다.

조작
----
WASD: 마우스 버튼을 누르지 않아도 카메라가 보는 방향 기준으로 자유 이동
Q/E: 월드 기준 아래/위 이동
Shift: 3배 빠르게 이동
마우스 오른쪽 버튼을 누른 채 이동: 현재 위치에서 시선 회전
마우스 휠: 보는 방향으로 앞/뒤 이동
Esc 또는 오른쪽 버튼 놓기: 마우스 잠금 해제
Art Direction의 Move Speed / Fast Move Multiplier / Look Sensitivity로 속도와 감도를 조절합니다.
1 / 2 / 3: 기본 시점 / 반대쪽 거리 / 마스코트 근접 시점
F1: 픽셀·윤곽선·인쇄 후처리 켜기/끄기 (표면 툰 셰이딩은 유지)
H: 화면 안내 숨기기/보이기
왼쪽 위 PRINT / VIEW 1·2·3 버튼으로도 효과와 시점을 바꿀 수 있습니다.
에디터에서 키보드가 반응하지 않으면 먼저 Game 화면 안을 클릭해 포커스를 주세요.

배달 트럭
---------
도로에 배치한 BBANG Delivery Truck은 민트색 운전석과 크림색 적재함을 가진 3D 모델입니다.
Assets/RetroToon/Prefabs/BBangDeliveryTruck.prefab을 다른 위치로 드래그해 재사용할 수 있습니다.
운전석, 적재함, 바퀴, 범퍼, 미러, 로고 등의 하위 오브젝트를 개별 편집할 수 있습니다.
전용 머티리얼과 메시는 Generated/Truck에 있습니다. 머티리얼의 Base Color로 색을 바꾸세요.
프리팹의 앞 방향은 로컬 +Z, 타이어 바닥은 로컬 Y=0입니다.
현재는 주차된 배치용 모델이며 차량 운전/물리 시뮬레이션은 포함하지 않습니다.
Tools > BBang > Add Delivery Truck to Street: 기존 데모 씬에 트럭 추가 (이미 있으면 선택).
Tools > BBang > Capture Delivery Truck: 거리 전경, 앞/뒤 근접 이미지를 Artifacts/RetroToon에 저장.

스타일 조절
-----------
Assets/RetroToon/Settings/RetroStyle.asset를 Inspector에서 선택하세요.
Pixel Height: 낮추면 픽셀이 커집니다. 기본 360 = 16:9에서 640x360 격자.
Outline Thickness: 가상 픽셀 기준 윤곽선 두께.
Depth Threshold / Normal Threshold: 실루엣 / 표면 꺾임 감지 민감도.
Dither Strength / Color Steps / Grain Strength: 디더링, 색 단계, 종이 입자.
Saturation / Warmth / Vignette Strength: 채도, 온기, 화면 가장자리 밝기.
Fog Color / Start / End: 거리 안개. 먼 윤곽선도 같은 안개에 묻힙니다.
Sun Color / Intensity: 주광 색과 세기.
Art Direction 오브젝트가 프로필을 읽어 Edit/Play 모드에 반영합니다.
하늘 그라데이션은 Settings/RetroSunset.mat의 Olive / Apricot / Horizon을 조절합니다.

다른 모델에 적용
---------------
머티리얼의 Shader를 BBang/Retro Toon으로 바꾸고 Base Color와 Base Map을 지정하세요.
텍스처가 있다면 Texture Strength를 1로, 단색이라면 0으로 설정합니다.
Shadow Color, Threshold, Softness, Strength로 그림자 색과 명암 경계를 조절합니다.
씬의 주 Directional Light와 실시간 그림자를 사용하며 일반 PBR 광택은 사용하지 않습니다.
추가 점광원/스포트라이트 조명, 투명 표면 전용 툰 렌더링은 구현 범위에 포함하지 않습니다.

프로젝트의 Graphics 기본 파이프라인과 두 Quality 단계에는
Settings/RetroPipeline.asset가 연결되어 있습니다.
이 파이프라인의 RetroRenderer에는 Full Screen Pass Renderer Feature가 있으며
RetroPrint.mat, Color Buffer Fetch, Depth + Normal, After Rendering Post Processing을 사용합니다.
Unity 6 Render Graph를 사용하며 Compatibility Mode를 켤 필요가 없습니다.

다른 URP 17 프로젝트로 가져가기
----------------------------
Artifacts/RetroToon/BBang-RetroToon.unitypackage를 Import하세요.
데모를 그대로 쓰려면 Graphics와 현재 Quality 단계의 Render Pipeline Asset을
Assets/RetroToon/Settings/RetroPipeline.asset로 지정하고 데모 씬을 여세요.
기존 렌더러를 유지하려면 위 Full Screen Pass 설정을 기존 Universal Renderer에 추가하고
RetroDemoController의 profile, printMaterial, sun, viewCamera 참조를 연결하세요.
Input System이 활성화되어야 데모 카메라 키보드/마우스 조작이 작동합니다.

구성
----
Shaders/RetroToon.shader: 2단 셀 명암, 유색 그림자, 안개, ShadowCaster/Depth/Normals.
Shaders/RetroPrint.shader: 깊이·노멀 윤곽선, 픽셀 격자, Bayer 디더링, 인쇄 색감.
Shaders/RetroSky.shader: 올리브와 살구색 하늘 그라데이션.
Shaders/RetroWorldText.shader: 차체나 벽 뒤에서 가려지는 입체 간판 글자.
Generated/: 저장된 3D 메시, 체크 텍스처, 팔레트 머티리얼.
Runtime/: Inspector 프로필, 데모 카메라, 간판 글꼴 연결.
Editor/: 씬 재생성, 1920x1080 캡처, 유효성 검사, 패키지 내보내기 메뉴.
ThirdParty/: GitHub 원본 스냅샷, 고정 커밋 정보, MIT 라이선스.

Tools > BBang > Build Retro Burger Street는 생성한 데모 씬과 Generated 자산을 재생성합니다.
수동으로 꾸민 데모를 유지하려면 먼저 씬을 다른 이름으로 복제하세요.
원래 Graphics/Quality/Build Settings 사본은 Artifacts/RetroToon/OriginalSettings에 있습니다.
카메라 캡처와 검사 결과는 Artifacts/RetroToon에 저장됩니다.

출처
----
표면 셰이더 기반: ColinLeung-NiloCat / UnityURPToonLitShaderExample (MIT)
https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample
commit fbe276e0df1d1f9379e13581c20b6a080a1a6977

픽셀화·디더링 기반: Kodrin / URP-PSX (MIT)
https://github.com/Kodrin/URP-PSX
commit 83670002c8893b0e9156c6145684697f38b712ec

두 공개 예제의 코드를 이 프로젝트의 Unity 6 / URP 17 렌더링에 맞게 수정했습니다.
유료 NiloToon 패키지는 포함하지 않습니다. 원본 저작권과 라이선스는 ThirdParty에 보존했습니다.
거리, 간판, 버거 마스코트, 배달 트럭은 이 프로젝트에서 코드로 생성한 새 3D 지오메트리입니다.
이 씬은 렌더링과 아트 스타일을 확인하는 데모이며 이동 캐릭터/게임 규칙은 포함하지 않습니다.
