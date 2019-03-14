﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PictureApp.API.Data;

namespace PictureApp.API.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20190312125127_ResetPasswordTokenAndExtendUser")]
    partial class ResetPasswordTokenAndExtendUser
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("PictureApp.API.Models.AccountActivationToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Token");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("AccountActivationTokens");
                });

            modelBuilder.Entity("PictureApp.API.Models.NotificationTemplate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Abbreviation");

                    b.Property<string>("Body");

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.Property<string>("Subject");

                    b.HasKey("Id");

                    b.ToTable("NotificationTemplates");
                });

            modelBuilder.Entity("PictureApp.API.Models.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateAdded");

                    b.Property<string>("Description");

                    b.Property<bool>("IsMain");

                    b.Property<string>("PublicId");

                    b.Property<string>("Url");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Photo");
                });

            modelBuilder.Entity("PictureApp.API.Models.ResetPasswordToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Token");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("ResetPasswordToken");
                });

            modelBuilder.Entity("PictureApp.API.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<bool>("IsAccountActivated");

                    b.Property<byte[]>("PasswordHash");

                    b.Property<byte[]>("PasswordSalt");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PictureApp.API.Models.UserFollower", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("FolloweeId");

                    b.Property<int>("FollowerId");

                    b.HasKey("Id");

                    b.HasIndex("FolloweeId");

                    b.HasIndex("FollowerId");

                    b.ToTable("UserFollower");
                });

            modelBuilder.Entity("PictureApp.API.Models.AccountActivationToken", b =>
                {
                    b.HasOne("PictureApp.API.Models.User", "User")
                        .WithOne("ActivationToken")
                        .HasForeignKey("PictureApp.API.Models.AccountActivationToken", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PictureApp.API.Models.Photo", b =>
                {
                    b.HasOne("PictureApp.API.Models.User", "User")
                        .WithMany("Photos")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PictureApp.API.Models.ResetPasswordToken", b =>
                {
                    b.HasOne("PictureApp.API.Models.User", "User")
                        .WithOne("ResetPasswordToken")
                        .HasForeignKey("PictureApp.API.Models.ResetPasswordToken", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PictureApp.API.Models.UserFollower", b =>
                {
                    b.HasOne("PictureApp.API.Models.User", "Followee")
                        .WithMany("Following")
                        .HasForeignKey("FolloweeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PictureApp.API.Models.User", "Follower")
                        .WithMany("Followers")
                        .HasForeignKey("FollowerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}