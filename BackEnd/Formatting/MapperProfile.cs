﻿using AutoMapper;
using Models;
using Models.Equipments;
using Models.Events;
using Models.People;
using Models.PublicAPI.Requests.Account;
using Models.PublicAPI.Requests.Equipment;
using Models.PublicAPI.Requests.Equipment.Equipment;
using Models.PublicAPI.Requests.Equipment.EquipmentType;
using Models.PublicAPI.Requests.Events.Event;
using Models.PublicAPI.Requests.Events.EventType;
using Models.PublicAPI.Requests.Roles;
using Models.PublicAPI.Responses.Equipment;
using Models.PublicAPI.Responses.Event;
using Models.PublicAPI.Responses.Login;
using Models.PublicAPI.Responses.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Formatting;
using Extensions;
using Models.DataBaseLinks;
using Models.PublicAPI.Requests;
using Models.PublicAPI.Requests.Events.Event.Create;
using Models.PublicAPI.Requests.Events.Event.Edit;
using BackEnd.Models;
using Models.PublicAPI.Responses.Event.Invitations;

namespace BackEnd.Formatting
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            EventEditMaps();
            Invitations();
            CreateMap<EquipmentTypeCreateRequest, EquipmentType>();
            CreateMap<EventTypeEditRequest, EventType>()
                .ForAllMembers(opt => opt.Condition(a =>
                    a.GetType().GetProperty(opt.DestinationMember.Name)?.GetValue(a) != null));

            CreateMap<EquipmentCreateRequest, Equipment>();
            CreateMap<Equipment, EquipmentView>();

            CreateMap<EquipmentType, EquipmentTypeView>();
            CreateMap<AccountCreateRequest, User>()
                .ForMember(u => u.UserName, map => map.MapFrom(ac => ac.Email));
            CreateMap<User, LoginResponse>();

            CreateMap<EquipmentEditRequest, Equipment>()
                .ForAllMembers(opt => opt.Condition(a =>
                    a.GetType().GetProperty(opt.DestinationMember.Name)?.GetValue(a) != null));


            CreateMap<EventTypeCreateRequest, EventType>();
            CreateMap<EventType, EventTypeView>();
            CreateMap<EventCreateRequest, Event>();
            CreateMap<Event, EventView>();
            var userId = default(Guid);
            CreateMap<Event, EventAndUserId>()
                .ForMember(evuid => evuid.UserId, map => map.MapFrom(ev => userId));

            CreateMap<EventAndUserId, CompactEventView>()
                .ForMember(cev => cev.ShiftsCount, map => map.MapFrom(ev => ev.Shifts.Count))
                .ForMember(cev => cev.TotalDurationInMinutes, map => map.MapFrom(ev =>
                    ev
                        .Shifts
                        .Select(s => s.EndTime.Subtract(s.BeginTime).TotalMinutes)
                        .Sum()))
                .ForMember(cev => cev.CurrentParticipantsCount, map => map.MapFrom(ev =>
                    ev.Shifts
                        .SelectMany(s => s.Places)
                        .SelectMany(p => p.PlaceUserRoles)
                        .Count(pur => pur.UserStatus == UserStatus.Accepted)))
                .ForMember(cev => cev.TargetParticipantsCount, map => map.MapFrom(ev =>
                    ev.Shifts
                        .SelectMany(s => s.Places)
                        .Sum(p => p.TargetParticipantsCount)))
                .ForMember(cev => cev.Participating, map => map.MapFrom(evuid =>
                    evuid
                        .Shifts
                        .SelectMany(s => s.Places)
                        .SelectMany(p => p.PlaceUserRoles)
                        .Where(pur => pur.UserStatus == UserStatus.Accepted)
                        .Any(pur => pur.UserId == evuid.UserId)));


            CreateMap<ShiftCreateRequest, Shift>();
            CreateMap<Shift, ShiftView>();


            CreateMap<Guid, PlaceUserRole>()
                .ForMember(pur => pur.UserId, map => map.MapFrom(pwr => pwr));
            CreateMap<Guid, PlaceEquipment>()
                .ForMember(pe => pe.EquipmentId, map => map.MapFrom(g => g));
            CreateMap<PlaceCreateRequest, Place>()
                .ForMember(p => p.PlaceEquipments, map => map.MapFrom(s => s.Equipment))
                .ForMember(p => p.PlaceUserRoles, map => map.MapFrom(pcr => pcr.Invited));

            CreateMap<Place, PlaceView>()
                .ForMember(p => p.Equipment, map => map.MapFrom(p =>
                    p.PlaceEquipments.Select(pe => pe.Equipment)
                ))
                .ForMember(p => p.Participants, map => map.MapFrom(p => p.PlaceUserRoles
                                                                   .Where(pur => pur.UserStatus == UserStatus.Accepted)))
                .ForMember(p => p.Invited, map => map.MapFrom(p => p.PlaceUserRoles
                                                               .Where(pur => pur.UserStatus == UserStatus.Invited)))
                .ForMember(p => p.Wishers, map => map.MapFrom(p => p.PlaceUserRoles
                                                              .Where(pur => pur.UserStatus == UserStatus.Wisher)))
                .ForMember(p => p.Unknowns, map => map.MapFrom(p => p.PlaceUserRoles
                                                               .Where(pur => pur.UserStatus == UserStatus.Unknown)));

            CreateMap<RoleCreateRequest, Role>();
            CreateMap<Role, RoleView>();

            CreateMap<PlaceUserRole, UserAndRole>();

            CreateMap<User, UserView>();
            CreateMap<UserSetting, UserSettingPresent>()
                .ForMember(usp => usp.Value, map => map.MapFrom(us => us.Value.ParseToJson()));
        }

        private void EventEditMaps()
        {
            CreateMap<List<ShiftEditRequest>, List<Shift>>()
                .ConvertUsing(new ListsConverter<ShiftEditRequest, Shift>(s => s.Id));
            CreateMap<EventEditRequest, Event>()
                .ForAllMembers(opt => opt.Condition(a =>
                    a.GetType().GetProperty(opt.DestinationMember.Name)?.GetValue(a) != null));

            CreateMap<List<PlaceEditRequest>, List<Place>>()
                .ConvertUsing(new ListsConverter<PlaceEditRequest, Place>(p => p.Id));
            CreateMap<ShiftEditRequest, Shift>()
                .ForAllMembers(opt => opt.Condition(a =>
                    a.GetType().GetProperty(opt.DestinationMember.Name)?.GetValue(a) != null));

            CreateMap<List<DeletableRequest>, List<PlaceEquipment>>()
                .ConvertUsing(new ListsConverter<DeletableRequest, PlaceEquipment>(eq => eq.EquipmentId));
            CreateMap<List<PersonWorkRequest>, List<PlaceUserRole>>()
                .ConvertUsing(new ListsConverter<PersonWorkRequest, PlaceUserRole>(pur => pur.UserId));
            CreateMap<PlaceEditRequest, Place>()
                .ForMember(p => p.PlaceEquipments, map => map.MapFrom(per => per.Equipment))
                .ForMember(p => p.PlaceUserRoles, map => map.MapFrom(per => per.Invited))
                .ForAllMembers(opt => opt.Condition(a =>
                    a.GetType().GetProperty(opt.DestinationMember.Name)?.GetValue(a) != null));
            CreateMap<DeletableRequest, PlaceEquipment>()
                .ForMember(pe => pe.EquipmentId, map => map.MapFrom(eer => eer.Id));
            CreateMap<PersonWorkRequest, PlaceUserRole>()
                .ForMember(pur => pur.UserId, map => map.MapFrom(pwr => pwr.Id));
        }

        private void Invitations()
        {
            CreateMap<EventAndUserId, InvitationsEventView>()
                .ForMember(iev => iev.PlaceId, map => map.MapFrom(euid =>
                    euid
                        .Shifts
                        .SelectMany(s => s.Places)
                        .SelectMany(p => p.PlaceUserRoles)
                        .Single(pur => pur.UserId == euid.UserId)
                        .PlaceId))
                .ForMember(iev => iev.ShiftDurationInMinutes, map => map.MapFrom(euid =>
                    euid
                        .Shifts
                        .Where(s => s
                               .Places
                               .Any(p => p.PlaceUserRoles
                                    .Any(pur => pur.UserId == euid.UserId)))
                                .Select(s => s.EndTime.Subtract(s.BeginTime).TotalMinutes)
                        .Single()))
                .ForMember(iev => iev.Role, map => map.MapFrom(euid =>
                   euid
                       .Shifts
                       .SelectMany(s => s.Places)
                       .SelectMany(p => p.PlaceUserRoles)
                       .Single(pur => pur.UserId == euid.UserId)
                       .Role));
        }
    }
}