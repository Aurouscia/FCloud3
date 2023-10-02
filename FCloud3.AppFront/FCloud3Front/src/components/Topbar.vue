<script setup lang="ts">

</script>

<template>
<div class="topbar">
  <div class="left">
    <div class="topbar-item">
      <div>
        <RouterLink to="/HomePage">主页</RouterLink>
      </div>
    </div>
  </div>
  <div class="right">
    <div class="topbar-item">
      <div>
        <RouterLink to="/User/Login">登录</RouterLink>
      </div>
    </div>
    <div class="topbar-item">
      <div>
        <RouterLink to="/User/Personal">个人</RouterLink>
      </div>
    </div>
  </div>
</div>
</template>

<style scoped>
.topbar{
  position: fixed;
  top:0px;
  left:0px;
  height: 50px;
  width: 100vw;
  overflow: visible;
  display: flex;
  justify-content: space-between;
  margin-right: auto;
  white-space: nowrap;
  background-color: white;
  box-shadow: 0px 2px 5px #aaa;
}
.left{
  display: flex;
  font-size: 20px;
}
.right{
  display: flex;
  font-size:16px
}
.topbar-item{
  padding: 0px 10px 0px 10px;
  position: relative;
}
.topbar-item>div{
  display: flex;
  height: 50px;
  justify-content: center;
  align-items: center;
}

a{
  color:unset;
  text-decoration: unset;
}
a:hover{
  text-decoration: underline;
}
</style>