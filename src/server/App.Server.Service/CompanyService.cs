﻿using System.Linq;

using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

using App.Domain;
using App.Domain.Contracts;
using App.Domain.Repo;


namespace App.Server.Service
{
    public class CompanyService : BaseService, ICompanyService
    {
        private readonly IEntityRepository<Company> _companyRepository;
        private readonly IEntityRepository<User> _userRepository;

        public CompanyService(IEntityRepository<Company> companyRepository,
                              IEntityRepository<User> userRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public string CreateCompany(CompanyDto dto)
        {
            if (string.IsNullOrEmpty(dto.AdminEmail)
                || string.IsNullOrEmpty(dto.Name))
            {
                return null;
            }

            var user = _userRepository.AsQueryable().FirstOrDefault(x => x.Email == dto.AdminEmail);
            if (user == null)
            {
                return null;
            }

            var company = _companyRepository.AsQueryable().FirstOrDefault(x => x.Name == dto.Name);
            if (company != null)
            {
                return null;
            }

            dto.AdminId = user.IdStr;
            var item = Mapper.Map<CompanyDto, Company>(dto);
            var result = _companyRepository.Save(item);
            if (result.Ok)
            {
                _userRepository.Update(Query<User>.EQ(x => x.Id, user.Id),
                    Update<User>.Set(x => x.CompanyId, item.IdStr).Set(x => x.CompanyName, item.Name));
            }

            return result.Ok ? item.IdStr : null;
        }

        public bool SetCustomerCustomFields(CustomFieldSettingDto dto)
        {
            ObjectId cId;
            if (string.IsNullOrEmpty(dto.CompanyId)
                || !ObjectId.TryParse(dto.CompanyId, out cId)
                || dto.CustomFieldDtos == null
                || !dto.CustomFieldDtos.Any())
            {
                return false;
            }

            var company = _companyRepository.AsQueryable().FirstOrDefault(x => x.Id == cId);
            if (company == null)
            {
                return false;
            }

            var mappedItems = dto.CustomFieldDtos.Select(Mapper.Map<CustomFieldDto, CustomField>).ToList();
            var result = _companyRepository.Update(Query<Company>.EQ(x => x.Id, cId), Update<Company>.Set(x => x.CustomFields, mappedItems));

            return result.Ok;
        }
    }
}