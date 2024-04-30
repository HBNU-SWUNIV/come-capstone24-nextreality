using System;

namespace NextReality.Data
{
    public class Schema
    {
        public string userId;
        public string message;
        public DateTime requestTime;
        public DateTime responseTime;

        public void SetData(string user, string text, DateTime reqTime, DateTime resTime)
        {
            userId = user;
            message = text;
            requestTime = reqTime;
            responseTime = resTime;
        }

        // 동기화 데이터를 파싱하는 템플릿 메서드
        public virtual void ParsingData(string message)
        {

        }
    }
}