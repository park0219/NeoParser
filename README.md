# NeoParser
네오그라프 사내 인트라넷을 파싱하여 사용자에게 새 글 알림을 주는 프로그램입니다.

해당 프로그램은 MSSQL 2012 데이터베이스를 사용하고 있습니다.\
테이블 생성문
```sql
CREATE TABLE board
(
    BOARD_SEQ   int identity
        constraint board_pk
            primary key nonclustered,
    BOARD_TITLE varchar(100),
    BOARD_URL   varchar(200),
    BOARD_NAME  varchar(20),
    BOARD_DATE  varchar(20),
    BOARD_CLIP  char,
    USER_ID     varchar(20)
)
go

exec sp_addextendedproperty 'MS_Description', '게시글', 'SCHEMA', 'NeoParser', 'TABLE', 'board'
go

exec sp_addextendedproperty 'MS_Description', '게시글 SEQ', 'SCHEMA', 'NeoParser', 'TABLE', 'board', 'COLUMN', 'BOARD_SEQ'
go

exec sp_addextendedproperty 'MS_Description', '게시글 제목', 'SCHEMA', 'NeoParser', 'TABLE', 'board', 'COLUMN', 'BOARD_TITLE'
go

exec sp_addextendedproperty 'MS_Description', '게시글 URL', 'SCHEMA', 'NeoParser', 'TABLE', 'board', 'COLUMN', 'BOARD_URL'
go

exec sp_addextendedproperty 'MS_Description', '게시글 작성자', 'SCHEMA', 'NeoParser', 'TABLE', 'board', 'COLUMN', 'BOARD_NAME'
go

exec sp_addextendedproperty 'MS_Description', '게시글 작성일자', 'SCHEMA', 'NeoParser', 'TABLE', 'board', 'COLUMN',
     'BOARD_DATE'
go

exec sp_addextendedproperty 'MS_Description', '게시글 스크랩 여부(YN)', 'SCHEMA', 'NeoParser', 'TABLE', 'board', 'COLUMN',
     'BOARD_CLIP'
go

exec sp_addextendedproperty 'MS_Description', '사용자  ID', 'SCHEMA', 'NeoParser', 'TABLE', 'board', 'COLUMN', 'USER_ID'
go

```
