using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

// 이 클래스는 MySQL에 접근하기 위한 "아주 간단한 래퍼(Helper)"입니다.
// - 생성자에서 받은 Connection String을 사용
// - QueryAsync : SELECT 결과를 DataTable로 받고 싶을 때 사용
// - ScalarAsync: 첫 번째 값만 필요할 때 사용 (로그인, 방 ID, 카운트 등)
// - ExecAsync  : INSERT/UPDATE/DELETE용 (영향받은 행 수 반환)
//
// 각 함수는 호출할 때마다:
//  1) MySqlConnection 생성
//  2) OpenAsync()로 연결
//  3) MySqlCommand에 SQL과 파라미터 설정
//  4) 실행 후 using 범위를 벗어나며 자동 Dispose
//  → 커넥션 풀 덕분에 성능은 충분히 괜찮음(과제 수준에는 충분)
public class Db
{
    // MySQL 접속 문자열 (Program.Main에서 주입)
    private readonly string _cs;

    // 생성자
    public Db(string cs) => _cs = cs;

    // --------------------------------------------------------------------
    // QueryAsync
    //
    // 용도:
    //   - SELECT 쿼리 결과를 DataTable로 받고 싶을 때 사용
    //
    // 매개변수:
    //   - sql : 실행할 SELECT 문
    //   - ps  : MySqlParameter 가변 인자 (예: new MySqlParameter("@id", 1))
    //
    // 반환값:
    //   - DataTable: 행(Row)과 열(Column) 구조로 결과를 담은 표
    //
    // 사용 예:
    //   var dt = await db.QueryAsync(
    //       "SELECT * FROM Chat WHERE chat_room_id=@r",
    //       new MySqlParameter("@r", roomId));
    // --------------------------------------------------------------------
    public async Task<DataTable> QueryAsync(string sql, params MySqlParameter[] ps)
    {
        // 1) MySQL 연결 객체 생성
        using var con = new MySqlConnection(_cs);

        // 2) 실제 DB 서버와 연결 (비동기)
        await con.OpenAsync();

        // 3) 쿼리와 연결을 가지는 MySqlCommand 생성
        using var cmd = new MySqlCommand(sql, con);

        // 4) 파라미터가 있다면 Command에 추가
        //    ps?.Length > 0 는 ps == null 체크도 겸함
        if (ps?.Length > 0) cmd.Parameters.AddRange(ps);

        // 5) DataAdapter를 이용해 DataTable에 결과를 채운다.
        //    - DataAdapter는 SELECT 수행 후 내부적으로 DataTable.Fill 호출
        using var da = new MySqlDataAdapter(cmd);

        // 6) 비어 있는 DataTable 생성 후, Fill로 결과 채워 넣기
        var dt = new DataTable();
        da.Fill(dt);

        // 7) 호출자에게 결과 반환
        return dt;
    }

    // --------------------------------------------------------------------
    // ScalarAsync
    //
    // 용도:
    //   - SELECT 결과 중 "첫 번째 행의 첫 번째 컬럼" 값만 필요할 때 사용
    //   - 예: 로그인 유저 id 조회, 방 존재 여부, COUNT(*), LAST_INSERT_ID() 등
    //
    // 매개변수:
    //   - sql : 실행할 SELECT 문
    //   - ps  : MySqlParameter 가변 인자
    //
    // 반환값:
    //   - object? : 결과가 없으면 null, 있으면 박싱된 값(object 형)
    //               (호출자가 Convert.ToInt32, Convert.ToDateTime 등으로 캐스팅)
    //
    // 사용 예:
    //   var idObj = await db.ScalarAsync(
    //       "SELECT id FROM Users WHERE login_id=@lid AND pw=@pw LIMIT 1",
    //       new MySqlParameter("@lid", loginId),
    //       new MySqlParameter("@pw", pw));
    // --------------------------------------------------------------------
    public async Task<object?> ScalarAsync(string sql, params MySqlParameter[] ps)
    {
        using var con = new MySqlConnection(_cs);
        await con.OpenAsync();

        using var cmd = new MySqlCommand(sql, con);
        if (ps?.Length > 0) cmd.Parameters.AddRange(ps);

        // ExecuteScalarAsync:
        //   - SELECT 결과 중 첫 번째 행의 첫 번째 컬럼만 가져오는 함수
        //   - 결과 없으면 null
        return await cmd.ExecuteScalarAsync();
    }

    // --------------------------------------------------------------------
    // ExecAsync
    //
    // 용도:
    //   - INSERT, UPDATE, DELETE 쿼리 실행
    //   - SELECT 처럼 결과를 DataTable로 받을 필요 없을 때 사용
    //
    // 매개변수:
    //   - sql : 실행할 DML 쿼리 (INSERT/UPDATE/DELETE)
    //   - ps  : MySqlParameter 가변 인자
    //
    // 반환값:
    //   - int : 영향받은 행(row) 수
    //           예) INSERT 1건 → 1, WHERE 조건 안 맞아서 변경 0건 → 0
    //
    // 사용 예:
    //   int affected = await db.ExecAsync(
    //       "UPDATE ChatRoom SET updated_at=NOW() WHERE id=@r",
    //       new MySqlParameter("@r", roomId));
    // --------------------------------------------------------------------
    public async Task<int> ExecAsync(string sql, params MySqlParameter[] ps)
    {
        using var con = new MySqlConnection(_cs);
        await con.OpenAsync();

        using var cmd = new MySqlCommand(sql, con);
        if (ps?.Length > 0) cmd.Parameters.AddRange(ps);

        // ExecuteNonQueryAsync:
        //   - SELECT가 아닌 쿼리 실행
        //   - 영향 받은 행 수 반환 (INSERT/UPDATE/DELETE)
        return await cmd.ExecuteNonQueryAsync();
    }
}
