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

        // ����ȭ �����͸� �Ľ��ϴ� ���ø� �޼���
        public virtual void ParsingData(string message)
        {

        }
    }
}