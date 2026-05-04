# Project : Clearth

### [Navigation]

상세한 설계 의도와 트러블 슈팅 사례는 [**노션 포트폴리오**](https://www.notion.so/G-Star-Clearth-350fa4e779b780a6a965f449dec8c11f?source=copy_link)에서 확인하실 수 있습니다.  
이 README는 노션 문서의 기술적 구현체를 빠르게 확인하기 위한 가이드라인입니다.

| 기능 구분 | 핵심 스크립트 / 폴더 위치 | 주요 구현 포인트 |
| :--- | :--- | :--- |
| **Player 시스템** | [/Scripts/Player](https://github.com/fasd0114/Clearth/tree/main/Clearth/Assets/Scripts/Player) | 상태 제어 및 물리 연산 로직 관리 |
| **BOSS AI** | [/Scripts/Boss](https://github.com/fasd0114/Clearth/tree/main/Clearth/Assets/Scripts/Boss) | FSM 기반의 모듈형 페이즈/패턴 관리 |
| **수학적 로직 기믹** | [/Scripts/Object/Vine](https://github.com/fasd0114/Clearth/tree/main/Clearth/Assets/Scripts/Object/Vine) | 포물선 운동 공식을 활용한 정밀한 도약 구현 |
| **튜토리얼 시스템** | [/Scripts/Object/Tutorial](https://github.com/fasd0114/Clearth/tree/main/Clearth/Assets/Scripts/Object/Tutorial) | RenderTexture 기반 애니메이션 튜토리얼 시스템 |

<br>
> 본 프로젝트는 Unity 2022.3.61f1 환경에서 C#을 사용하여 개발되었습니다.*
