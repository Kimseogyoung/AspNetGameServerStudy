# GameServerStudy_Web
ASP.Net GameServer Study
------------------------

#배운 것들 정리하는 프로젝트
#현업에서 사용하던 코드와 다르게 개발중입니다.


------------------------
메모
TODO 
- cmd창에서 간이 클라이언트로 동작하도록 툴 만들기
- Protocol 자동 생성 솔루션 적용 (1순위 : ProtoBuf, 2순위 : MsgPack)
- Enum, Proto 제너레이터
- DB 스키마 관리
- 에러 리포트 시스템 구축

------------------------
클래스 용도
- Controller : Api처리 (Service호출 )
- Sevice : Api 각 함수 처리(Controller와 역할이 겹치는데 Sevice는 배치처리 대비용)
- ??? : 모델별로 검증 및 UserRepo 호출 (여기서 필요한 변수 저장) -> 네이밍 정리 필요, 예시 프로젝트는 Infomation... 뭐이런거인듯. 
- UserRepo : 실제 쿼리 및 캐싱


- Middleware : 요청 정보 로드 및 압축 암호화, 디시리얼라이즈
- Filter : 로그, 인증, 트랜잭션 등
- ExceptionHandler : 에러 잡아서 ResultCode로 리턴 


----------------------
- Config
    - yaml
- Secret
    - ...
- DB
    - MySql
    - 형상관리 : liquibase (https://www.liquibase.com/download)
- Cache
    - Redis
