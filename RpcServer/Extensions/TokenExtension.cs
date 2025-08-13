using ModalStrikeServer.RpcServer.Utilities;

namespace ModalStrikeServer.RpcServer.Extensions {
    public static class TokenExtension {
        public static string CreateNewTicket(string authId) {
            if(!string.IsNullOrEmpty(authId))
                return EncryptUtility.MD5(authId + (DateTime.UtcNow.Ticks - GetRandomDate().Ticks));

            throw new ArgumentNullException(nameof(authId));
        }

        private static DateTime GetRandomDate() {
            var random = new Random();
            long range = (DateTime.Today - new DateTime(1, 1, 1)).Days;
            var randomDay = random.Next((int)range);
            return new DateTime(1, 1, 1).AddDays(randomDay);
        }

    }
}