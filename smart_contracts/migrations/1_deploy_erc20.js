var erc20 = artifacts.require("./StandardERC20.sol");
module.exports = function(deployer) {
      deployer.deploy(erc20, 10000000000);
}