﻿using FCloud3.Entities.Corr;
using FCloud3.Entities.TextSection;
using FCloud3.Repos.Cor;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using FCloud3.Repos.TextSec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.TextSec
{
    public class TextSectionService
    {
        private readonly CorrRepo _corrRepo;
        private readonly TextSectionRepo _textSectionRepo;
        public TextSectionService(CorrRepo corrRepo, TextSectionRepo textsectionRepo)
        {
            _corrRepo = corrRepo;
            _textSectionRepo = textsectionRepo;
        }

        public TextSection? GetById(int id)
        {
            return _textSectionRepo.GetById(id);
        }

        public static bool ModelCheck(TextSection section, out string? errmsg)
        {
            errmsg = null;
            if (string.IsNullOrEmpty(section.Title))
            {
                errmsg = "标题不能为空";
                return false;
            }
            return true;
        }
        /// <summary>
        /// 新建一个文本段
        /// </summary>
        /// <returns>新建的文本段Id</returns>
        public int TryAdd(int user, out string? errmsg)
        {
            TextSection newSection = new()
            {
                Title = "新建文本段",
                Content = "",
                ContentBrief = "",
                CreatorUserId = user
            };
            if (!ModelCheck(newSection, out errmsg))
                return 0;
            if (!_textSectionRepo.TryAdd(newSection, out errmsg))
                return 0;
            return newSection.Id;
        }
        /// <summary>
        /// 新建一个文本段并关联指定段落
        /// </summary>
        /// <returns>新建的文本段Id</returns>
        public int TryAddAndAttach(int user, int corrId, out string? errmsg)
        {
            Corr corr = _corrRepo.GetById(corrId) ?? throw new Exception("找不到指定Id的段落");
            if (corr.CorrType != CorrType.TextSection_WikiItem)
                throw new Exception("段落类型异常");
            int createdTextId = TryAdd(user, out errmsg);
            if (createdTextId <= 0)
                return 0;
            corr.A = createdTextId;
            _corrRepo.TryEdit(corr, out _);
            return createdTextId;
        }
        /// <summary>
        /// 更新一个文本段
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        public bool TryUpdate(int id,int user,string? title, string? content, out string? errmsg)
        {
            if (id == 0)
            {
                throw new Exception("未得到更新文本段Id");
            }
            else
            {
                errmsg = null;
                if(title is not null)
                {
                    if (!_textSectionRepo.TryChangeTitle(id, title, out errmsg))
                        return false;
                }
                if(content is not null)
                {
                    if (!_textSectionRepo.TryChangeContent(id, content, out errmsg))
                        return false;
                }
                return true;
            }
        }
    }
}
