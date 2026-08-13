using System;

namespace ServerCore.Model
{
    /// <summary>
    /// 모델이 DB 테이블로서 갖는 메타데이터. ClassGenerator 가 CSV 로부터 생성한다.
    /// 손으로 붙이지 말 것 — Data/Excel/Model/** 를 고치고 생성기를 돌린다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class EntityAttribute : Attribute
    {
        /// <summary>
        /// 기본 키 컬럼. CSV Key 컬럼의 pk 토큰에서 나오며, 순서는 CSV 행 순서다.
        /// </summary>
        public string[] Pk { get; set; }

        /// <summary>
        /// User 스코프 안에서 행을 소유자별로 가르는 컬럼. User 계열에만 있다.
        /// Auth/Center 에는 ambient 소유자 개념이 없어 null 이다.
        /// </summary>
        public string ScopeKey { get; set; }
    }
}
