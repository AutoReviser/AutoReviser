namespace AutoReviser
{
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Immutable;

    public record SeatWithRecord
    {
        public SeatWithRecord(int row, int column, bool isReserved)
        {
            (Row, Column, IsReserved) = (row, column, isReserved);
        }

        public int Row { get; }

        public int Column { get; }

        public bool IsReserved { get; }
    }

    public record SeatWithPositionalRecord(int Row, int Column, bool IsReserved);

    public record Screening(
        Guid Id,
        Guid TheaterId,
        ImmutableArray<SeatWithPositionalRecord> Seats,
        DateTime ScreeningTime,
        decimal DefaultFare,
        decimal ChildFare);

    [TestClass]
    public class Reviser_specs
    {
        [TestMethod]
        public void sut_supports_records()
        {
            var seed = new SeatWithRecord(row: 1, column: 2, isReserved: false);
            SeatWithRecord actual = seed.Revise(x => x.IsReserved == true);
            actual.IsReserved.Should().Be(true);
        }

        [TestMethod]
        public void sut_supports_positional_records()
        {
            var seed = new SeatWithPositionalRecord(Row: 1, Column: 2, IsReserved: false);
            SeatWithPositionalRecord actual = seed.Revise(x => x.IsReserved == true);
            actual.IsReserved.Should().Be(true);
        }

        [TestMethod, AutoData]
        public void sut_supports_deep_properties_of_records(
            Guid screeingId,
            Guid theaterId,
            DateTime screeningTime,
            decimal defaultFee)
        {
            var seed = new Screening(
                Id: screeingId,
                theaterId,
                Seats: ImmutableArray.Create(
                    new SeatWithPositionalRecord(Row: 1, Column: 1, IsReserved: false),
                    new SeatWithPositionalRecord(Row: 1, Column: 2, IsReserved: false)),
                screeningTime,
                defaultFee,
                ChildFare: defaultFee / 2);

            Screening actual = seed.Revise(x => x.Seats[1].IsReserved == true);

            actual.Seats[1].IsReserved.Should().BeTrue();
        }
    }
}
