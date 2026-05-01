Project : Clearth

G-Star 전시회 출품작으로, C#과 Unity를 활용해 개발한 2D 액션 플랫포머 게임 프로젝트입니다.

포트폴리오 상세 링크

상세 기술 문서 : https://www.notion.so/G-Star-Clearth-350fa4e779b780a6a965f449dec8c11f?source=copy_link

주요 구현 기능

Player 시스템 : https://github.com/fasd0114/Clearth/tree/main/Clearth/Assets/Scripts/Player
- 캐릭터의 각종 로직을 관리합니다.
- 사이드 스크롤러 특유의 조작감을 위해 물리 연산과 애니메이션을 동기화했습니다.
- Hitbox/Hurtbox를 물리적으로 분리하여 독립적인 데이터 튜닝 환경을 구축하였으며 조작감 개선 또는 밸런스 수정 시의 유지보수 비용을 최소화했습니다.
 
BOSS AI : https://github.com/fasd0114/Clearth/tree/main/Clearth/Assets/Scripts/Boss
- FSM 로직을 바탕으로 보스의 페이즈 전환 및 패턴 AI를 구현했습니다.

수학적 로직 기반의 정밀 기믹 구현 : https://github.com/fasd0114/Clearth/tree/main/Clearth/Assets/Scripts/Object/Vine
- 수학적 로직을 기반으로 플레이어가 덩굴에서 이탈할 때 정확한 힘과 속도로 점프할 수 있도록 했습니다.

RenderTexture 기반 Tutorial 시스템 : https://github.com/fasd0114/Clearth/tree/main/Clearth/Assets/Scripts/Object/Tutorial
- 이기종 렌더링 통합 가이드를 위해 렌더 텍스처를 활용한 시각적 보조 시스템을 구현했습니다.
- 플레이 환경별 해상도 변화와 무관하게 튜토리얼 가이드가 항상 일정한 비율로 출력되도록 튜토리얼 전용 카메라 시스템을 구축했습니다.
