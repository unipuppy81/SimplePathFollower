# Simple Path Follower
**Simple Path Follower**는 유니티에서 베지어(Bézier) 곡선 기반의 이동 경로를 시각적으로 설계하고 관리하는 강력한 도구입니다. 데이터 중심 설계(`ScriptableObject`)를 통해 경로를 자산화하고, 다양한 오브젝트에 유연하게 적용할 수 있습니다.



## 💠 1. Introduction
유니티 애니메이션 키프레임 방식은 곡선 경로에서의 일정한 속도 유지나 복잡한 궤적 수정이 어렵습니다. 
이 프로젝트는 **베지어 곡선 수식**을 활용하여 에디터 상에서 직관적으로 경로를 시각화하고, 개발자가 이동 속도와 방향(LookAt)을 정교하게 제어할 수 있는 환경을 제공합니다.

---

## 💠 2. 특징 (Features)
* **Visual Editing:** 씬(Scene) 뷰에서 핸들을 조작하여 직관적으로 곡선 경로 설계 가능
* **Data-Driven:** `ScriptableObject`를 사용하여 경로 데이터를 파일로 저장 및 재사용
* **Flexible Speed:** 곡선 구간별로 속도를 다르게 설정하거나, 일괄적으로 동일하게 변경 가능
* **Intelligent Rotation:** 진행 방향을 자동으로 바라보거나, 특정 타겟(`LookAt`)을 고정해서 응시 가능
* **Loop System:** 경로의 끝에서 처음으로 돌아가는 루프 이동 지원
* **Decoupled Architecture:** `PathManager`를 통해 여러 경로를 리스트로 관리하고, 원하는 타이밍에 특정 경로로 이동 실행 가능

---

## 💠 3. Component Guide

### 🔘 Path Creator & Data
경로의 뼈대를 설계하고 저장하는 핵심 파트입니다.
* **Create Data:** `Create > PathSystem > PathData` 메뉴로 데이터 에셋을 생성합니다.
* **Edit Mode:** `Shift + Click`으로 노드를 추가하고, `Mirrored/Free` 모드를 통해 탄젠트를 조작합니다.
* **Batch Tools:** 전체 경로의 위치를 옮기는 `Offset` 기능과 모든 구간의 속도를 한 번에 맞추는 `Batch Speed` 기능을 지원합니다.

### 🔘 Path Follower
오브젝트의 실제 움직임을 담당합니다.
* **LookAt:** 지정된 Transform이 있으면 그곳을 바라보고, 비어있으면 진행 방향(`Forward`)을 바라봅니다.
* **Segment Resolution:** 경로의 부드러움 정도를 결정합니다.
* **Event System:** 노드 도달 시 문자열 기반 이벤트를 발생시켜 사운드나 이펙트와 연동할 수 있습니다.

### 🔘 Path Manager (Example)
여러 경로와 팔로워를 통합 제어하는 예시 클래스입니다.
* **Path List:** 사용할 여러 `PathDataAsset`을 리스트로 관리합니다.
* **Trigger:** 숫자키 입력 등 원하는 타이밍에 특정 인덱스의 경로를 팔로워에게 전달하여 이동을 시작합니다.

---

## 💠 4. 사용 방법 (How to Use)

1.  **데이터 생성:** `Project` 창에서 `Create > Path System > PathData`를 선택하여 `ScriptableObject`를 생성합니다.
2.  **경로 설계:** 빈 오브젝트에 `PathCreator`를 부착하고 생성한 경로 데이터를 할당한 뒤, 씬 뷰에서 경로를 수정하고 **Save** 버튼을 눌러 저장합니다.
3.  **팔로워 설정:** 이동할 오브젝트에 `Path Follower` 스크립트를 부착합니다. (필요 시 `LookAt` 타겟 지정)
4.  **매니저 실행:** `PathManager`에 경로 에셋들을 리스트에 넣고 이동할 오브젝트를 참조한 뒤, 런타임에서 경로 이동을 실행합니다.



---

## 💠 5. 활용 예시 (Use Cases)
* **컷신 카메라 (Cutscene Camera):** 부드러운 카메라 무빙과 특정 지점에서의 시선 고정 연출
* **투사체 및 이동 패턴:** 유도 미사일이나 특정 궤적을 그리며 이동하는 적 유닛
* **레이싱/레일 게임:** 정해진 트랙을 따라 일정한 속도로 달리는 기차나 자동차
* **환경 연출:** 배경에서 정해진 경로를 순찰하는 NPC나 이동하는 발판

---

## Developer
**김대연 (Dae-yeon Kim)**
* [GitHub Profile](https://github.com/unipuppy81)
