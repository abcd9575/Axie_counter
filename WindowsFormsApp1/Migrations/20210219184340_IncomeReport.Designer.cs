﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WindowsFormsApp1.Database;

namespace WindowsFormsApp1.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20210219184340_IncomeReport")]
    partial class IncomeReport
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.11");

            modelBuilder.Entity("WindowsFormsApp1.Models.IncomeReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccountName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<long>("Income")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Server")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Total")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("IncomeReports");
                });

            modelBuilder.Entity("WindowsFormsApp1.Models.ItemOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Stat")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ItemOptions");
                });

            modelBuilder.Entity("WindowsFormsApp1.Models.PriceInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemName")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<string>("Server")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("PriceInfo");
                });

            modelBuilder.Entity("WindowsFormsApp1.Models.PriceInfo2ItemOption", b =>
                {
                    b.Property<int>("ItemOptionId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PriceInfoId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("ItemOptionId", "PriceInfoId");

                    b.HasIndex("PriceInfoId");

                    b.ToTable("PriceInfoItemOption");
                });

            modelBuilder.Entity("WindowsFormsApp1.Models.PriceInfo2ItemOption", b =>
                {
                    b.HasOne("WindowsFormsApp1.Models.ItemOption", "ItemOption")
                        .WithMany("PriceInfo2ItemOptions")
                        .HasForeignKey("ItemOptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WindowsFormsApp1.Models.PriceInfo", "PriceInfo")
                        .WithMany("PriceInfo2ItemOptions")
                        .HasForeignKey("PriceInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
