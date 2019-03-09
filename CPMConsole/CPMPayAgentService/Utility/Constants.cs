using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    public class IntegerConstants
    {
        public static int SUCCESS = 7; //= успешно, "-1" 
        public static int IN_PROCESS = 1; //= v rabote,
        public static int ERROR = -1; //= ошибка, 
        public static int WRONG_PARAMS = -400;//не указаны необходимые параметры запроса, 
        public static int DUPLICATE_SESSION_ID = -404;//= такой sessionId уже есть, 
        public static int WRONG_CHECKSUM = -500; ///"-500" = Не верная контрольная сумма
        public static int WRONG_TIME = -403; ///"-403" = Не верное время
        public static int WRONG_PASS = 2; ///"2" = Неправильный пароль карты
        public static int PREPARED = 5; //// "5" = Platezh podgotovlen
        public static int NO_SUCH_SESSION = -405; //// "-405" = po takoy sessii podgotovki operacii net
    }

    public class StringConstants
    {
        public static string LocalTcpPort = "LocalTcpPort";
        public static string EMVLoadOperation = "<EMVRequest type=\"Load\">"+
"<emv_account_id>{0}</emv_account_id>"+
"</EMVRequest>";
        public static string EMVUnLoadOperation = "<EMVRequest type=\"UnLoad\">" +
"<emv_account_id>{0}</emv_account_id>" +
"</EMVRequest>";

        /*
         * <!-- 
          Операция	Команда изменения статуса
          DF822B	Статус карты
          DF802E	Hot status карты
          DF8175
          Заблокировать карту по причине “Украдена”	STCC01	CRST0	CHST7
          Заблокировать карту по причине “Утеряна”	STCC01	CRST0	CHST6
          Разблокировать карту	STCC03 ???	CRST0	CHST0
          -->
         */
        public static string EMVBlockUnblockOperation = "<EMVRequest type=\"CreateRequest\">"+
  @"<client_data_id>{0}</client_data_id>
  <emv_account_id>{1}</emv_account_id>
  <emv_card_id>{2}</emv_card_id>
  <application_type_id>BTRT15</application_type_id>
  <application_subtype_id>{3}</application_subtype_id>
  <Ext_Data>
    <card_status_id>CRST0</card_status_id>
    <hot_card_status_id>{4}</hot_card_status_id>
  </Ext_Data>
</EMVRequest>";
    }
}
