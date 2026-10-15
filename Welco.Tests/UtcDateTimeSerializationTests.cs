using System;
using System.Text.Json;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.JsonConverters;
using Xunit;

namespace Welco.Tests
{
    public class UtcDateTimeSerializationTests
    {
        private readonly JsonSerializerOptions _options;

        public UtcDateTimeSerializationTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new UtcDateTimeJsonConverter());
            _options.Converters.Add(new NullableUtcDateTimeJsonConverter());
        }

        [Fact]
        public void DateTime_SerializesWithZOnEnd_WhenUtc()
        {
            var dt = new DateTime(2026, 10, 15, 14, 40, 38, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(dt, _options);

            Assert.StartsWith("\"2026-10-15T14:40:38", json);
            Assert.EndsWith("Z\"", json);
        }

        [Fact]
        public void DateTime_SerializesWithZOnEnd_WhenUnspecified()
        {
            // Database DateTime values typically have DateTimeKind.Unspecified
            var dt = new DateTime(2026, 10, 15, 14, 40, 38, DateTimeKind.Unspecified);
            var json = JsonSerializer.Serialize(dt, _options);

            Assert.StartsWith("\"2026-10-15T14:40:38", json);
            Assert.EndsWith("Z\"", json);
        }

        [Fact]
        public void NullableDateTime_SerializesWithZOnEnd_WhenHasValue()
        {
            DateTime? dt = new DateTime(2026, 10, 15, 14, 40, 38, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(dt, _options);

            Assert.EndsWith("Z\"", json);

            DateTime? nullDt = null;
            var nullJson = JsonSerializer.Serialize(nullDt, _options);
            Assert.Equal("null", nullJson);
        }

        [Fact]
        public void AuditLogDto_CreatedAt_SerializesWithZOnEnd()
        {
            var dto = new AuditLogDto
            {
                Id = Guid.NewGuid(),
                Action = "LOGIN",
                EntityName = "User",
                EntityId = "user-123",
                PerformedBy = "admin@welco.com",
                CreatedAt = new DateTime(2026, 10, 15, 14, 40, 38, DateTimeKind.Unspecified)
            };

            var json = JsonSerializer.Serialize(dto, _options);
            using var doc = JsonDocument.Parse(json);
            var createdAtProp = doc.RootElement.GetProperty("CreatedAt").GetString();

            Assert.NotNull(createdAtProp);
            Assert.EndsWith("Z", createdAtProp);
            Assert.StartsWith("2026-10-15T14:40:38", createdAtProp);
        }
    }
}