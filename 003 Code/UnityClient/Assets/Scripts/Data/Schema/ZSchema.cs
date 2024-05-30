using System;

namespace NextReality.Data.Schema
{
    public abstract class ZSchema
    {
        public string userId;
        public string message;
        public DateTime requestTime;
        public DateTime responseTime;

        public DateTime messageTime;
		public string isSuccess;

		public ZSchema() { }

		public ZSchema(string message)
		{
			ParsingData(message);
		}

        protected ProtocolConverter GetProtocolStream(string message = null)
        {
            return GetProtocolStreamByIndividual(new ProtocolConverter(message).CastString(ref userId).CastMiliSeconds(ref messageTime));
        }

		// ���������� ���赵�� ���
		protected abstract ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev);


		// ����ȭ �����͸� �Ľ��ϴ� ���ø� �޼���
		public void ParsingData(string message)
        {
            this.message = message;
			// requestTime = DateTime.UtcNow.AddHours(9); // FIXME
            requestTime = DateTime.UtcNow; // FIXME

            GetProtocolStream(message).Cast(ref isSuccess);
        }

		// ����ȭ �����͸� ����ȭ�ϴ� ���ø� �޼���
		public string StringifyData()
		{
            /*
			responseTime = DateTime.UtcNow.AddHours(9); //FIXME

            userId = Managers.User?.Id ?? "testID";
            messageTime = DateTime.UtcNow.AddHours(9);
            */

            responseTime = DateTime.UtcNow; //FIXME

            userId = Managers.User?.Id ?? "testID";
            messageTime = DateTime.UtcNow;

            return SchemaType + ProtocolConverter.commandSeparator + GetProtocolStream().ToStream();
		}

        public abstract string SchemaType { get; }
	}
}