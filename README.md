# omok-20
멀티플레이 오목 게임
<br>
 
## 프로젝트 소개
유니티와 Node.js 서버를 활용하여 멀티 플레이가 가능한 오목게임 입니다. 
플레이어의 급수에 맞는 상대를 매칭해 주고, 맞는 상대가 없을 경우 ai와 대전할 수 있습니다.
<br>
 
## 개발 기간
- 25.09.10 ~ 25.09.21
<br>

## 사용한 협업 툴
- [서버 Github](https://github.com/ReDvGaMe/omok-server-20)
- [Figma](https://www.figma.com/design/psX8HtU66rIbb0ZO5YONFp/Omok-20?node-id=112-2&t=FzBD7VAVAx7KFoKn-0)
- [Jira](https://entozforme.atlassian.net/wiki/spaces/2/overview?homepageId=3604720)
<br>

## 멤버 구성
- 강희영 : 메인 화면 UI, 게임 화면 UI, 기보 로직 및 씬 개발, UI 시스템 구조 설계
- 이은비 : 로그인 화면, 회원가입 화면, 사운드 설정 (싱글톤 패턴을 통해 씬이 변경되어도 사운드 값이 유지되도록 하고, PlayerPref을 통해 기기에 사운드 정보 저장)
- 이건희 : 오목 게임 로직 개발 (착수 기능, 승/패/무, 렌주룰 적용 등)
- 이채연 : 오목 AI 개발 (Minimax 함수, Alpha Beta Pruning을 통한 시간 성능 개선 등)
- 김주영 : 서버/네트워크 개발 (실시간 대전 및 멀티플레이, 사용자 인증 및 데이터 관리 API 연동, 결과 및 랭킹 시스템 연동)
<br>

## 개발 환경
- 유니티 : 6000.2.3f1
- Node.js
<br>

## 주요 기능 
[오목게임_세부기능.pdf](https://github.com/user-attachments/files/22614269/_.pdf)
- 급수 시스템
- 멀티 플레이
- AI 플레이
- 게임 플레이 (렌주룰에 의한 게임 로직)
- 랭킹 시스템
- 기보 시스템
- 로그인/회원가입 기능
