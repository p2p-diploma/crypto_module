// SPDX-License-Identifier: MIT
pragma solidity >=0.4.22 < 0.9.0;

contract EthereumTransferContract {
    mapping(address => mapping(address => uint)) slots;

    modifier is_sumInBlock(address recipient) {
        require(slots[msg.sender][recipient] > 0, "The payment is not in block");
        _;
    }
    event Transfered(address indexed sender, address indexed recipient, uint amount);
    event Blocked(address indexed sender, address indexed recipient, uint amount);
    event Reverted(address indexed sender, address indexed recipient, uint amount);

    function block_sum(address recipient) public payable {
        require(recipient != address(0), "Recipient's address is invalid");
        require(msg.value > 0, "Amount of ether must be greater than 0");
        slots[msg.sender][recipient] = msg.value;
        emit Blocked(msg.sender, recipient, msg.value);
    }

    function final_transfer(address recipient) public is_sumInBlock(recipient) {
        uint amount = slots[msg.sender][recipient];
        payable(recipient).transfer(amount);
        delete slots[msg.sender][recipient];
        emit Transfered(msg.sender, recipient, amount);
    }
    function revert_transfer(address recipient) public is_sumInBlock(recipient) {
        uint amount = slots[msg.sender][recipient];
        payable(msg.sender).transfer(amount);
        delete slots[msg.sender][recipient];
        emit Reverted(msg.sender, recipient, amount);
    }
}